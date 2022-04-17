using VkActivity.Service.Models;

namespace VkActivity.Service.Abstractions;

public interface IVkIntegration
{
    Task<List<VkApiUser>> GetUsersActivityAsync(int[] userIds);
    Task<List<VkApiUser>> GetUsersAsync(int[] userIds);
}
