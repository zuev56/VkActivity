using VkActivity.Data.Models;
using Zs.Common.Abstractions;

namespace VkActivity.Worker.Abstractions;

public interface IUserManager
{
    Task<IOperationResult<List<User>>> AddUsersAsync(params string[] screenNames);
    Task<IOperationResult> UpdateUsersAsync(params int[] userIds);
}
