using System.Collections.Generic;
using VkActivity.Data.Models;

namespace VkActivity.Api.Models;

public sealed class ActivityLogPage
{
    public ushort Page { get; set; }
    public List<ActivityLogItem> Items { get; set; } = new();
}
