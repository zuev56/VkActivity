using VkActivity.Data.Models;
using Zs.Common.Abstractions;

namespace VkActivity.Service;

public interface IActivityLoggerService
{
    /// <summary> Add new Vk user ID</summary>
    /// <param name="userIds">VK user ID</param>
    [Obsolete("Move to API")]
    Task<IOperationResult<List<User>>> AddNewUsersAsync(params int[] userIds);

    /// <summary> Activity data collection </summary>
    Task<IOperationResult> SaveVkUsersActivityAsync();
}
