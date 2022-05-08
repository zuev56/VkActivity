using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using VkActivity.Service.Abstractions;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace VkActivity.Service.Services
{
    public class UserManager : IUserManager
    {
        private readonly IUsersRepository _usersRepo;
        private readonly IVkIntegration _vkIntegration;
        private readonly ILogger<UserManager> _logger;

        public UserManager(
            IUsersRepository usersRepo,
            IVkIntegration vkIntegration,
            ILogger<UserManager> logger)
        {
            _usersRepo = usersRepo ?? throw new ArgumentNullException(nameof(usersRepo));
            _vkIntegration = vkIntegration ?? throw new ArgumentNullException(nameof(vkIntegration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <param name="userIds">User IDs or ScreenNames</param>
        public async Task<IOperationResult<List<User>>> AddUsersAsync(params string[] screenNames)
        {
            ArgumentNullException.ThrowIfNull(nameof(screenNames));

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
                var savedSuccessfully = !usersForSave.Any()
                    || await _usersRepo.SaveRangeAsync(usersForSave).ConfigureAwait(false);

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

        public async Task<IOperationResult> UpdateUsersAsync(params int[] userIds)
        {
            ArgumentNullException.ThrowIfNull(nameof(userIds));

            var existingDbUserIds = await _usersRepo.FindExistingIdsAsync(userIds).ConfigureAwait(false);
            var userIdsToUpdate = existingDbUserIds.Select(id => id.ToString()).ToArray();

            var vkUsers = await _vkIntegration.GetUsersWithFullInfoAsync(userIdsToUpdate).ConfigureAwait(false);
            if (vkUsers is null)
                return ServiceResult<List<User>>.Error("Vk API error: cannot get users by IDs");

            var dbUsers = vkUsers.Select(u => Mapper.ToUser(u));

            return await _usersRepo.UpdateRangeAsync(dbUsers).ConfigureAwait(false);
        }
    }
}
