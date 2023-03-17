using Microsoft.Extensions.Logging;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Models;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace VkActivity.Common.Services;

public sealed class UserManager : IUserManager
{
    private readonly IUsersRepository _usersRepo;
    private readonly IVkIntegration _vkIntegration;
    private readonly ILogger<UserManager> _logger;

    public UserManager(
        IUsersRepository usersRepo,
        IVkIntegration vkIntegration,
        ILogger<UserManager> logger)
    {
        _usersRepo = usersRepo;
        _vkIntegration = vkIntegration;
        _logger = logger;
    }

    /// <param name="screenNames">User IDs or ScreenNames</param>
    public async Task<Result<List<User>>> AddUsersAsync(params string[] screenNames)
    {
        ArgumentNullException.ThrowIfNull(screenNames);

        var resultUsersList = new List<User>();
        var result = Result.Success(resultUsersList);
        try
        {
            var newUserIds = await SeparateNewUserIdsAsync(screenNames).ConfigureAwait(false);
            if (!newUserIds.Any())
            {
                return Result.Fail<List<User>>("Users already exist in DB");
            }

            var newUserStringIds = newUserIds.Select(x => x.ToString()).ToArray();
            var vkUsers = await _vkIntegration.GetUsersWithFullInfoAsync(newUserStringIds).ConfigureAwait(false);

            //if (userIds.Except(newUserIds).Any())
            //    result.AddMessage($"Existing users won't be added. Existing users IDs: {string.Join(',', userIds.Except(newUserIds))}", InfoMessageType.Warning);

            var usersForSave = vkUsers.Select(Mapper.ToUser).ToArray();
            var savedSuccessfully = !usersForSave.Any() || await _usersRepo.SaveRangeAsync(usersForSave).ConfigureAwait(false);
            if (!savedSuccessfully)
            {
                return Result.Fail<List<User>>("User saving failed");
            }

            resultUsersList.AddRange(usersForSave);
            return result;

        }
        catch (Exception ex)
        {
            _logger.LogErrorIfNeed(ex, "New users saving failed");
            return Result.Fail<List<User>>("New users saving failed");
        }
    }

    private async Task<int[]> SeparateNewUserIdsAsync(string[] sourceScreenNames)
    {
        var vkApiUsers = await _vkIntegration.GetUsersWithActivityInfoAsync(sourceScreenNames).ConfigureAwait(false);
        var userIds = vkApiUsers.Select(u => u.Id).ToArray();
        var existingDbUsers = await _usersRepo.FindAllByIdsAsync(userIds).ConfigureAwait(false);
        var newUserIds = userIds.Except(existingDbUsers.Select(u => u.Id)).ToArray();

        return newUserIds;
    }

    public async Task<Result> UpdateUsersAsync(params int[] userIds)
    {
        if (userIds.Length == 0)
        {
            return Result.Fail($"{nameof(userIds)} is null or empty");
        }

        var existingDbUserIds = await _usersRepo.FindExistingIdsAsync(userIds).ConfigureAwait(false);
        if (existingDbUserIds.Length < userIds.Length)
        {
            var nonexistentIds = string.Join(',', userIds.Except(existingDbUserIds));
            return Result.Fail($"Unable to update users that are not exist in database. IDs: {nonexistentIds}");
        }

        var userIdsToUpdate = existingDbUserIds.Select(id => id.ToString()).ToArray();
        var vkUsers = await _vkIntegration.GetUsersWithFullInfoAsync(userIdsToUpdate).ConfigureAwait(false);
        if (!vkUsers.Any())
        {
            return Result.Fail("Vk API error: cannot get users by IDs");
        }

        var dbUsers = vkUsers.Select(Mapper.ToUser);
        var updateResult = await _usersRepo.UpdateRangeAsync(dbUsers).ConfigureAwait(false);

        return updateResult;
    }

    public async Task<Result<List<User>>> AddFriendsOf(int userId)
    {
        try
        {
            var friendIds = await _vkIntegration.GetFriendIds(userId);
            var stringNames = friendIds.Select(friendIds => friendIds.ToString()).ToArray();

            return await AddUsersAsync(stringNames);
        }
        catch (Exception ex)
        {
            _logger.LogErrorIfNeed(ex, "Add friends failed");
            return Result.Fail<List<User>>("Add friends failed");
        }
    }

    public async Task<Result<User>> GetUserAsync(int userId)
    {
        var user = await _usersRepo.FindByIdAsync(userId);
        return user ?? Result.Fail<User>("Not Found");
    }
}