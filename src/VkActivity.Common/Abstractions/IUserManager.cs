using VkActivity.Data.Models;
using Zs.Common.Abstractions;

namespace VkActivity.Common.Abstractions;

public interface IUserManager
{
    Task<IOperationResult<List<User>>> AddUsersAsync(params string[] screenNames);
    Task<IOperationResult> UpdateUsersAsync(params int[] userIds);
    Task<IOperationResult<List<User>>> AddFriendsOf(int userId);
    Task<IOperationResult<User>> GetUserAsync(int userId);
}
