using VkActivity.Service.Models.VkApi;

namespace VkActivity.Service.Abstractions;

public interface IVkIntegration
{
    Task<List<VkApiUser>> GetUsersWithActivityInfoAsync(int[] userIds);
    Task<List<VkApiUser>> GetUsersWithFullInfoAsync(int[] userIds);
}
