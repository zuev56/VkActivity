using VkActivity.Data.Models;

namespace VkActivity.Service.Models;

public sealed class ActivityLogPage
{
    public ushort Page { get; set; }
    public List<ActivityLogItem>? Items { get; set; }
}
