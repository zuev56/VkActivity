using VkActivity.Data.Models;

namespace VkActivity.Service.Models;

public class UserWithActivity
{
    public User User { get; init; }
    public int ActivitySec { get; init; }
    public bool isOnline { get; init; }
}
