using VkActivity.Data.Models;

namespace VkActivity.Api.Models;

public class SimpleActivity
{
    public int UserId { get; init; }
    public string? UserName { get; init; }
    public string? CurrentStatus { get; init; }
    public int VisitsCount { get; init; }
    [Obsolete("Use TimeOnPlatforms instead")]
    public TimeSpan TimeInSite { get; init; }
    [Obsolete("Use TimeOnPlatforms instead")]
    public TimeSpan TimeInApp { get; init; }
    public Dictionary<Platform, TimeSpan> TimeOnPlatforms { get; init; } = new();
}
