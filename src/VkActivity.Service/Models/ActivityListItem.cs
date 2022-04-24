using VkActivity.Data.Models;

namespace VkActivity.Service.Models;

/// <summary>
/// Used to show users list with their status and activity time
/// </summary>
public sealed class ActivityListItem
{
    public User? User { get; init; }
    public int ActivitySec { get; init; }
    public bool IsOnline { get; init; }
}
