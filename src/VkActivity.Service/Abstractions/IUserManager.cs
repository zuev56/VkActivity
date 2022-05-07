using VkActivity.Data.Models;
using Zs.Common.Abstractions;

namespace VkActivity.Service.Abstractions
{
    public interface IUserManager
    {
        Task<IOperationResult<List<User>>> AddUsersAsync(params string[] screenNames);
        Task<IOperationResult<List<User>>> UpdateUsersAsync(params int[] userIds);

    }
}
