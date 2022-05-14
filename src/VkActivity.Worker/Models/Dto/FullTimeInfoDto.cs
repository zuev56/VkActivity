namespace VkActivity.Worker.Models.Dto;

public sealed class FullTimeInfoDto : PeriodInfoDto
{
    public int AnalyzedDaysCount { get; init; }
    public int ActivityDaysCount { get; init; }
    public int VisitsFromSite { get; init; }
    public int VisitsFromApp { get; init; }
    public string? AvgDailyTime { get; init; }
}
