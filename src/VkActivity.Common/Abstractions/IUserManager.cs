using VkActivity.Data.Models;
using Zs.Common.Models;

namespace VkActivity.Common.Abstractions;

public interface IUserManager
{
    Task<Result<List<User>>> AddUsersAsync(params string[] screenNames);
    Task<Result> UpdateUsersAsync(params int[] userIds);
    Task<Result<List<User>>> AddFriendsOf(int userId);
    Task<Result<User>> GetUserAsync(int userId);
}