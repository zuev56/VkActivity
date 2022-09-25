using System.Text.Json;
using VkActivity.Data.Models;

namespace VkActivity.Api.Models;

public class DetailedActivity
{
    public int UserId { get; init; }
    public string? UserName { get; init; }
    public string? Url { get; init; }
    public int AnalyzedDaysCount { get; init; }
    public int ActivityDaysCount { get; init; }
    public List<VisitInfo> VisitInfos { get; set; } = new();
    public TimeSpan FullTime => VisitInfos.Sum(i => i.Time);
    public int AllVisitsCount => VisitInfos.Sum(i => i.Count);
    public TimeSpan AvgDailyTime => ActivityDaysCount > 0 ? FullTime / ActivityDaysCount : default;

    /// <summary> Day-activity map for all time </summary>
    //public Dictionary<DateTime, TimeSpan>? ActivityCalendar { get; init; }
    //public TimeSpan MinDailyTime { get; init; }
    //public TimeSpan MaxDailyTime { get; init; }
    //public static ReadOnlyDictionary<DayOfWeek, TimeSpan> AvgWeekDayActivity { get; } = new ReadOnlyDictionary<DayOfWeek, TimeSpan>(_avgWeekDayActivity);
    //private static readonly Dictionary<DayOfWeek, TimeSpan> _avgWeekDayActivity = new()
    //{
    //    { DayOfWeek.Monday, TimeSpan.FromSeconds(-1) },
    //    { DayOfWeek.Tuesday, TimeSpan.FromSeconds(-1) },
    //    { DayOfWeek.Wednesday, TimeSpan.FromSeconds(-1) },
    //    { DayOfWeek.Thursday, TimeSpan.FromSeconds(-1) },
    //    { DayOfWeek.Friday, TimeSpan.FromSeconds(-1) },
    //    { DayOfWeek.Saturday, TimeSpan.FromSeconds(-1) },
    //    { DayOfWeek.Sunday, TimeSpan.FromSeconds(-1) }
    //};

    public DetailedActivity(User user)
    {
        UserId = user.Id;
        UserName = $"{user.FirstName} {user.LastName}";
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