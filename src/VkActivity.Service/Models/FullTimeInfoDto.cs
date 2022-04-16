namespace VkActivity.Service.Models;

public class FullTimeInfoDto
{
    public int AnalyzedDaysCount { get; init; }
    public int ActivityDaysCount { get; init; }
    public int VisitsFromSite { get; init; }
    public int VisitsFromApp { get; init; }
    public int VisitsCount { get; init; }
    public string? TimeInSite { get; init; }
    public string? TimeInApp { get; init; }
    public string? FullTime { get; init; }
    public string? AvgDailyTime { get; init; }
}
