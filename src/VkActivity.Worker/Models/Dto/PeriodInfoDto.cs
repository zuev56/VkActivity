using VkActivity.Data.Models;

namespace VkActivity.Worker.Models.Dto;

public class PeriodInfoDto
{
    public int UserId { get; init; }
    public string? UserName { get; init; }
    public int VisitsCount { get; init; }
    [Obsolete("Use TimeOnPlatforms instead")]
    public string? TimeInSite { get; init; }
    [Obsolete("Use TimeOnPlatforms instead")]
    public string? TimeInApp { get; init; }
    public Dictionary<Platform, string> TimeOnPlatforms { get; init; } = new();
    public string? FullTime { get; init; }
}
