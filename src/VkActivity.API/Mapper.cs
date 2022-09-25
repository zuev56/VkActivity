using VkActivity.Api.Models;
using VkActivity.Api.Models.Dto;
using Zs.Common.Extensions;

namespace VkActivity.Api;

public static class Mapper
{
    public static ListUserDto ToListUserDto(ActivityListItem activityListItem)
    {
        ArgumentNullException.ThrowIfNull(nameof(activityListItem));

        return new ListUserDto
        {
            Id = activityListItem.User!.Id,
            Name = $"{activityListItem.User!.FirstName} {activityListItem.User.LastName}",
            IsOnline = activityListItem.IsOnline,
            ActivitySec = activityListItem.ActivitySec,
        };
    }

    //public static PeriodInfoDto ToPeriodInfoDto(SimpleActivity simpleActivity)
    //{
    //    ArgumentNullException.ThrowIfNull(nameof(simpleActivity));

    //    return new PeriodInfoDto
    //    {
    //        UserId = simpleActivity.UserId,
    //        UserName = simpleActivity.UserName,
    //        VisitInfos = simpleActivity.VisitInfos.ToDtos(),
    //        AllVisitsCount = simpleActivity.VisitsCount,
    //        FullTime = simpleActivity.FullTime.ToDayHHmmss()
    //    };
    //}

    public static PeriodInfoDto ToFullTimeInfoDto(DetailedActivity detailedActivity)
    {
        ArgumentNullException.ThrowIfNull(nameof(detailedActivity));

        return new PeriodInfoDto
        {
            UserId = detailedActivity.UserId,
            UserName = detailedActivity.UserName,
            VisitInfos = detailedActivity.VisitInfos.ToDtos(),
            AllVisitsCount = detailedActivity.AllVisitsCount,
            FullTime = detailedActivity.FullTime.ToDayHHmmss(),
            AvgDailyTime = detailedActivity.AvgDailyTime.ToDayHHmmss(),
            ActivityDaysCount = detailedActivity.ActivityDaysCount,
            AnalyzedDaysCount = detailedActivity.AnalyzedDaysCount
        };
    }

    private static List<VisitInfoDto> ToDtos(this List<VisitInfo> visitInfos)
        => visitInfos.Select(vi => new VisitInfoDto
        {
            Platform = vi.Platform.ToString(),
            Count = vi.Count,
            Time = vi.Time.ToDayHHmmss()
        })
        .ToList();
}
