using System.Collections.ObjectModel;
using System.Text.Json;
using VkActivity.Data.Models;

namespace VkActivity.Api.Models;

public class DetailedActivity
{
    public int UserId { get; init; }
    public string? UserName { get; init; }
    public string? Url { get; init; }

    private static readonly Dictionary<DayOfWeek, TimeSpan> _avgWeekDayActivity = new()
    {
        { DayOfWeek.Monday, TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Tuesday, TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Wednesday, TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Thursday, TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Friday, TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Saturday, TimeSpan.FromSeconds(-1) },
        { DayOfWeek.Sunday, TimeSpan.FromSeconds(-1) }
    };

    public int AnalyzedDaysCount { get; init; }
    public int ActivityDaysCount { get; init; }
    public int VisitsFromSite { get; init; }
    public int VisitsFromApp { get; init; }
    public int VisitsCount => VisitsFromSite + VisitsFromApp;
    [Obsolete("Use TimeOnPlatforms instead")]
    public TimeSpan TimeInSite { get; init; }
    [Obsolete("Use TimeOnPlatforms instead")]
    public TimeSpan TimeInApp { get; init; }
    public Dictionary<Platform, TimeSpan> TimeOnPlatforms { get; init; } = new();
    public TimeSpan FullTime => TimeOnPlatforms.Sum(i => i.Value);

    /// <summary> Day-activity map for all time </summary>
    public Dictionary<DateTime, TimeSpan>? ActivityCalendar { get; init; }

    public TimeSpan AvgDailyTime => ActivityDaysCount > 0 ? FullTime / ActivityDaysCount : default;
    public TimeSpan MinDailyTime { get; init; }
    public TimeSpan MaxDailyTime { get; init; }
    public static ReadOnlyDictionary<DayOfWeek, TimeSpan> AvgWeekDayActivity { get; } = new ReadOnlyDictionary<DayOfWeek, TimeSpan>(_avgWeekDayActivity);

    public DetailedActivity(User user)
    {
        UserId = user.Id;
        UserName = $"{user.FirstName} {user.LastName}";
        AnalyzedDaysCount = 0;
        ActivityDaysCount = 0;
        VisitsFromSite = 0;
        VisitsFromApp = 0;
        //ActivityCalendar  = GetActivityForEveryDay(orderedLogForUser), Пока не используется
        TimeInSite = TimeSpan.Zero;
        TimeInApp = TimeSpan.Zero;
        Url = $"https://vk.com/id{JsonDocument.Parse(user.RawData).RootElement.GetProperty("id")}";
    }
}

// Tmp
public static class TimeSpanExtensions
{
    public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
    {
        return source.Select(selector).Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
    }
}