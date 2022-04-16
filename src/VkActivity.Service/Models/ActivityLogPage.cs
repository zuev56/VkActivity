using VkActivity.Data.Models;

namespace VkActivity.Service.Models;

public partial class ActivityLogPage
{
    public ushort Page { get; set; }
    public List<ActivityLogItem>? Items { get; set; }
}
