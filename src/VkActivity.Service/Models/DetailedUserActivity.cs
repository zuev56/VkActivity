using System.Collections.ObjectModel;

namespace VkActivity.Service.Models;

public class DetailedUserActivity
{
    public string? UserName { get; init; }
    public string? Url { get; init; }

    private static Dictionary<DayOfWeek, TimeSpan> _avgWeekDayActivity = new Dictionary<DayOfWeek, TimeSpan>
    {
        { DayOfWeek.Monday,    TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Tuesday,   TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Wednesday, TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Thursday,  TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Friday,    TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Saturday,  TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Sunday,    TimeSpan.FromSeconds(-1) }
    };

    public int AnalyzedDaysCount { get; init; }
    public int ActivityDaysCount { get; init; }
    public int VisitsFromSite { get; init; }
    public int VisitsFromApp { get; init; }
    public int VisitsCount => VisitsFromSite + VisitsFromApp;
    public TimeSpan TimeInSite { get; init; }
    public TimeSpan TimeInApp { get; init; }
    public TimeSpan FullTime => TimeInSite + TimeInApp;

    /// <summary> Day-activity map for all time </summary>
    public Dictionary<DateTime, TimeSpan>? ActivityCalendar { get; init; }

    public TimeSpan AvgDailyTime => ActivityDaysCount > 0 ? (TimeInSite + TimeInApp) / ActivityDaysCount : default;
    public TimeSpan MinDailyTime { get; init; }
    public TimeSpan MaxDailyTime { get; init; }
    public ReadOnlyDictionary<DayOfWeek, TimeSpan> AvgWeekDayActivity { get; } = new ReadOnlyDictionary<DayOfWeek, TimeSpan>(_avgWeekDayActivity);

}
