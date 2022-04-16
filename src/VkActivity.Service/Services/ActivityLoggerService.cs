using System.Globalization;
using Microsoft.EntityFrameworkCore;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.WebAPI;

namespace VkActivity.Service.Services;

public class ActivityLoggerService : IActivityLoggerService
{
    private readonly IConfiguration _configuration;
    private readonly IActivityLogItemsRepository _activityLogRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly IVkIntegration _vkIntegration;
    private readonly ILogger<ActivityLoggerService> _logger;

    public ActivityLoggerService(
        IActivityLogItemsRepository activityLogRepo,
        IUsersRepository usersRepo, 
        IVkIntegration vkIntegration,
        ILogger<ActivityLoggerService> logger)
    {
        _activityLogRepo = activityLogRepo ?? throw new ArgumentNullException(nameof(activityLogRepo));
        _usersRepo = usersRepo ?? throw new ArgumentNullException(nameof(usersRepo));
        _vkIntegration = vkIntegration ?? throw new ArgumentNullException(nameof(vkIntegration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IOperationResult<List<User>>> AddNewUsersAsync(params int[] userIds)
    {
        var resultUsersList = new List<User>();
        var result = ServiceResult<List<User>>.Success(resultUsersList);
        try
        {
            var response = await _vkIntegration.GetUsers(userIds);

            if (response is null)
                return ServiceResult<List<User>>.Error("Response is null");

            var existingDbUsers = await _usersRepo.FindAllByIdsAsync(userIds);

            if (existingDbUsers?.Count > 0)
                result.AddMessage($"Existing users won't be added. Existing users IDs: {string.Join(',', existingDbUsers.Select(u => u.Id))}", InfoMessageType.Warning);

            var usersForSave = response.Users.Where(u => !existingDbUsers.Select(eu => eu.Id).Contains(u.Id)).Select(u => (User)u);
            var savedSuccessfully = await _usersRepo.SaveRangeAsync(usersForSave);

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

    /// <inheritdoc/>
    public async Task<IOperationResult> SaveVkUsersActivityAsync()
    {
        ServiceResult result = ServiceResult.Success();
        try
        {
            var vkUsers = await _usersRepo.FindAllAsync();
            var userIds = vkUsers.Select(u => u.Id).ToArray();

            if (!userIds.Any())
            {
                result.AddMessage("There are no users in the database", InfoMessageType.Warning);
                return result;
            }

            var response = await _vkIntegration.GetUsersActivity(userIds);

            if (response is null)
            {
                bool saveResult = await SetUndefinedActivityToAllVkUsers();
                _logger?.LogWarning("Set undefined activity to all VkUsers (succeeded: {SaveResult})", saveResult);
                result.AddMessage(InfoMessage.Warning("Response is null. Setting undefined activity to all VkUsers"));
                return result;
            }

            int loggedItemsCount = await LogVkUsersActivityAsync(response.Users);

            result.AddMessage(InfoMessage.Success($"Logged {loggedItemsCount} activities"));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogErrorIfNeed(ex, "SaveVkUsersActivityAsync error");
            return ServiceResult.Error("Users activity saving error");
        }
    }

    /// <summary>Save undefined user activities to database</summary>
    private async Task<bool> SetUndefinedActivityToAllVkUsers()
    {
        var users = await _usersRepo.FindAllAsync();

        var lastUsersActivityLogItems = await _activityLogRepo.FindLastUsersActivity();

        var activityLogItems = new List<ActivityLogItem>();
        foreach (var user in users)
        {
            if (lastUsersActivityLogItems.First(i => i.UserId == user.Id).IsOnline == true)
                activityLogItems.Add(
                    new ActivityLogItem
                    {
                        UserId = user.Id,
                        IsOnline = null,
                        IsOnlineMobile = false,
                        OnlineApp = null,
                        LastSeen = -1,
                        InsertDate = DateTime.UtcNow
                    }
                );
        }

        return await _activityLogRepo.SaveRangeAsync(activityLogItems);
    }

    /// <summary>Save user activities to database</summary>
    /// <param name="apiUsers">All users current state from VK API</param>
    /// <returns>Logged <see cref="ActivityLogItem">s count</returns>
    private async Task<int> LogVkUsersActivityAsync(List<VkApiUser> apiUsers)
    {
        // TODO: Add user activity info (range) - ???
        var lastActivityLogItems = await _activityLogRepo.FindLastUsersActivity();
        var activityLogItemsForSave = new List<ActivityLogItem>();

        foreach (var apiUser in apiUsers)
        {
            // When account is deleted or banned or smth else
            if (apiUser.LastSeenUnix == null)
                continue;

            var lastActivityLogItem = lastActivityLogItems.FirstOrDefault(i => i.UserId == apiUser.Id);
            var currentOnlineStatus = apiUser.Online == 1;
            var currentMobileStatus = apiUser.OnlineMobile == 1;
            var currentApp = apiUser.OnlineApp;

            if (lastActivityLogItem == null
                || lastActivityLogItem.IsOnline != currentOnlineStatus
                || lastActivityLogItem.IsOnlineMobile != currentMobileStatus
                || lastActivityLogItem.OnlineApp != currentApp)
            {
                // Vk corrects LastSeen, so we have to work with logged value, not current API value
                int lastSeenForLog = apiUser.LastSeenUnix?.Time ?? 0;
                if (lastActivityLogItem != null && apiUser.LastSeenUnix != null)
                    lastSeenForLog = Math.Max(lastActivityLogItem.LastSeen, apiUser.LastSeenUnix.Time);

                activityLogItemsForSave.Add(
                    new ActivityLogItem
                    {
                        UserId = apiUser.Id,
                        IsOnline = currentOnlineStatus,
                        IsOnlineMobile = currentMobileStatus,
                        OnlineApp = apiUser.OnlineApp,
                        LastSeen = lastSeenForLog,
                        InsertDate = DateTime.UtcNow
                    });
            }
        }

        return await _activityLogRepo.SaveRangeAsync(activityLogItemsForSave)
            ? activityLogItemsForSave.Count
            : -1;
    }

}
