using VkActivity.Service.Models;

namespace VkActivity.Service.Abstractions;

public interface IVkIntegration
{
    Task<VkApiResponse> GetUsersActivity(int[] userIds);
    Task<VkApiResponse> GetUsers(int[] userIds);
}
