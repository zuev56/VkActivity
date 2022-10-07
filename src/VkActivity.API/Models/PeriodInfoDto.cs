namespace VkActivity.Api.Models.Dto;

public class PeriodInfoDto
{
    public int UserId { get; init; }
    public string UserName { get; init; } = null!;
    public List<VisitInfoDto> VisitInfos { get; set; } = new();
    public int AllVisitsCount { get; init; }
    public string? FullTime { get; init; }
    public int AnalyzedDaysCount { get; init; }
    public int ActivityDaysCount { get; init; }
    public string? AvgDailyTime { get; init; }
}
