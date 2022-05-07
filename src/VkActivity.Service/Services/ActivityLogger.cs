using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.VkApi;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace VkActivity.Service.Services;

public sealed class ActivityLogger : IActivityLogger
{
    private readonly IActivityLogItemsRepository _activityLogRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly IVkIntegration _vkIntegration;
    private readonly ILogger<ActivityLogger> _logger;

    public ActivityLogger(
        IActivityLogItemsRepository activityLogRepo,
        IUsersRepository usersRepo,
        IVkIntegration vkIntegration,
        ILogger<ActivityLogger> logger)
    {
        _activityLogRepo = activityLogRepo ?? throw new ArgumentNullException(nameof(activityLogRepo));
        _usersRepo = usersRepo ?? throw new ArgumentNullException(nameof(usersRepo));
        _vkIntegration = vkIntegration ?? throw new ArgumentNullException(nameof(vkIntegration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    /// <param name="userIds">User IDs or ScreenNames</param>
    public async Task<IOperationResult<List<User>>> AddNewUsersAsync(params string[] screenNames)
    {
        var resultUsersList = new List<User>();
        var result = ServiceResult<List<User>>.Success(resultUsersList);
        try
        {
            (int[] userIds, int[] newUserIds) = await SeparateNewUserIdsAsync(screenNames).ConfigureAwait(false);

            if (!newUserIds.Any())
                return ServiceResult<List<User>>.Warning("Users already exist in DB");

            var newUserStringIds = newUserIds.Select(x => x.ToString()).ToArray();
            var vkUsers = await _vkIntegration.GetUsersWithFullInfoAsync(newUserStringIds).ConfigureAwait(false);
            if (vkUsers is null)
                return ServiceResult<List<User>>.Error("Vk API error: cannot get users by IDs");

            if (userIds.Except(newUserIds).Any())
                result.AddMessage($"Existing users won't be added. Existing users IDs: {string.Join(',', userIds.Except(newUserIds))}", InfoMessageType.Warning);

            var usersForSave = vkUsers.Select(u => Mapper.ToUser(u));
            var savedSuccessfully = usersForSave.Any()
                ? await _usersRepo.SaveRangeAsync(usersForSave).ConfigureAwait(false)
                : true;

            if (savedSuccessfully)
            {
                resultUsersList.AddRange(usersForSave);
                return result;
            }
            else
                return ServiceResult<List<User>>.Error("User saving failed");
        }
        catch (Exception ex)
        {
            _logger.LogErrorIfNeed(ex, "New users saving failed");
            return ServiceResult<List<User>>.Error("New users saving failed");
        }
    }

    private async Task<(int[] userIds, int[] newUserIds)> SeparateNewUserIdsAsync(string[] sourceScreenNames)
    {
        var vkApiUsers = await _vkIntegration.GetUsersWithActivityInfoAsync(sourceScreenNames).ConfigureAwait(false);
        var userIds = vkApiUsers.Select(u => u.Id).ToArray();

        var existingDbUsers = await _usersRepo.FindAllByIdsAsync(userIds).ConfigureAwait(false);

        var newUserIds = userIds.Except(existingDbUsers.Select(u => u.Id)).ToArray();

        return (userIds, newUserIds);
    }

    /// <inheritdoc/>
    public async Task<IOperationResult> SaveVkUsersActivityAsync()
    {
        ServiceResult result = ServiceResult.Success();
        try
        {
            var userIds = await _usersRepo.FindAllIdsAsync().ConfigureAwait(false);

            if (!userIds.Any())
            {
                result.AddMessage("There are no users in the database", InfoMessageType.Warning);
                return result;
            }

            var userStringIds = userIds.Select(id => id.ToString()).ToArray();
            var vkUsers = await _vkIntegration.GetUsersWithActivityInfoAsync(userStringIds).ConfigureAwait(false);

            int loggedItemsCount = await LogVkUsersActivityAsync(vkUsers).ConfigureAwait(false);

#if DEBUG
            Trace.WriteLine($"LoggedItemsCount: {loggedItemsCount}");
#endif

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogErrorIfNeed(ex, "SaveVkUsersActivityAsync error");

            await TrySetUndefinedActivityToAllVkUsers().ConfigureAwait(false);

            return ServiceResult.Error("Users activity saving error");
        }
    }

    /// <summary>Save undefined user activities to database</summary>
    private async Task TrySetUndefinedActivityToAllVkUsers()
    {
        try
        {
            var users = await _usersRepo.FindAllAsync();

            var lastUsersActivityLogItems = await _activityLogRepo.FindLastUsersActivity();

            if (!lastUsersActivityLogItems.Any())
                return;

            var activityLogItems = new List<ActivityLogItem>();
            foreach (var user in users)
            {
                if (lastUsersActivityLogItems.First(i => i.UserId == user.Id).IsOnline == true)
                    activityLogItems.Add(
                        new ActivityLogItem
                        {
                            UserId = user.Id,
                            IsOnline = null,
                            Platform = 0,
                            LastSeen = -1,
                            InsertDate = DateTime.UtcNow
                        }
                    );
            }

            var saveResult = await _activityLogRepo.SaveRangeAsync(activityLogItems);

            _logger.LogWarningIfNeed("Set undefined activity to all VkUsers (succeeded: {SaveResult})", saveResult);
        }
        catch (Exception)
        {
            _logger.LogErrorIfNeed("Set undefined activity to all VkUsers error");
            throw;
        }
    }

    /// <summary>Save user activities to database</summary>
    /// <param name="apiUsers">All users current state from VK API</param>
    /// <returns>Logged <see cref="ActivityLogItem">s count</returns>
    private async Task<int> LogVkUsersActivityAsync(List<VkApiUser> apiUsers)
    {
        // TODO: Add user activity info (range) - ???
        var lastActivityLogItems = await _activityLogRepo.FindLastUsersActivity().ConfigureAwait(false);
        var activityLogItemsForSave = new List<ActivityLogItem>();

        foreach (var apiUser in apiUsers)
        {
            // When account is deleted or banned or smth else
            if (apiUser.LastSeen == null)
                continue;

            var lastActivityLogItem = lastActivityLogItems.FirstOrDefault(i => i.UserId == apiUser.Id);
            var currentPlatform = apiUser.LastSeen.Platform;
            var currentIsOnline = apiUser.IsOnline == 1;

            if (lastActivityLogItem == null
                || lastActivityLogItem.IsOnline != currentIsOnline
                || lastActivityLogItem.Platform != currentPlatform)
            {
                // Vk corrects LastSeen, so we have to work with logged value, not current API value
                int lastSeenForLog = apiUser.LastSeen?.UnixTime ?? 0;
                if (lastActivityLogItem != null && apiUser.LastSeen != null)
                    lastSeenForLog = Math.Max(lastActivityLogItem.LastSeen, apiUser.LastSeen.UnixTime);

                activityLogItemsForSave.Add(
                    new ActivityLogItem
                    {
                        UserId = apiUser.Id,
                        IsOnline = currentIsOnline,
                        Platform = currentPlatform,
                        LastSeen = lastSeenForLog,
                        InsertDate = DateTime.UtcNow
                    });
            }
        }

        if (activityLogItemsForSave.Any())
        {
            return await _activityLogRepo.SaveRangeAsync(activityLogItemsForSave).ConfigureAwait(false)
                ? activityLogItemsForSave.Count
                : -1;
        }

        return 0;
    }

}
