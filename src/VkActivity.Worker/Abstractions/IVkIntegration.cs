using VkActivity.Worker.Models.VkApi;

namespace VkActivity.Worker.Abstractions;

public interface IVkIntegration
{
    /// <param name="screenNames">User IDs or ScreenNames</param>
    Task<List<VkApiUser>> GetUsersWithActivityInfoAsync(string[] screenNames);

    /// <param name="screenNames">User IDs or ScreenNames</param>
    Task<List<VkApiUser>> GetUsersWithFullInfoAsync(string[] screenNames);

    /// <summary>Get friend IDs for specific user</summary>
    Task<int[]> GetFriendIds(int userId);
}