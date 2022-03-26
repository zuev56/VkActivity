namespace VkActivity.Data.Models;

public partial class ActivityLogPage
{
    public ushort Page { get; set; }
    public List<ActivityLogItem>? Items { get; set; }
}
