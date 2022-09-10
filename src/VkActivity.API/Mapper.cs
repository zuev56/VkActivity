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

    public static PeriodInfoDto ToPeriodInfoDto(SimpleActivity simpleActivity)
    {
        ArgumentNullException.ThrowIfNull(nameof(simpleActivity));

        return new PeriodInfoDto
        {
            UserId = simpleActivity.UserId,
            UserName = simpleActivity.UserName,
            VisitsCount = simpleActivity.VisitsCount,
            TimeInSite = simpleActivity.TimeInSite.ToDayHHmmss(),
            TimeInApp = simpleActivity.TimeInApp.ToDayHHmmss(),
            TimeOnPlatforms = simpleActivity.TimeOnPlatforms.ToDictionary(i => i.Key, i => i.Value.ToDayHHmmss()),
            FullTime = simpleActivity.TimeOnPlatforms.Sum(i => i.Value).ToDayHHmmss()
        };
    }

    public static FullTimeInfoDto ToFullTimeInfoDto(DetailedActivity detailedActivity)
    {
        ArgumentNullException.ThrowIfNull(nameof(detailedActivity));

        return new FullTimeInfoDto
        {
            UserId = detailedActivity.UserId,
            UserName = detailedActivity.UserName,
            VisitsCount = detailedActivity.VisitsCount,
            TimeInSite = detailedActivity.TimeInSite.ToDayHHmmss(),
            TimeInApp = detailedActivity.TimeInApp.ToDayHHmmss(),
            TimeOnPlatforms = detailedActivity.TimeOnPlatforms.ToDictionary(i => i.Key, i => i.Value.ToDayHHmmss()),
            FullTime = detailedActivity.FullTime.ToDayHHmmss(),
            AvgDailyTime = detailedActivity.AvgDailyTime.ToDayHHmmss()
        };
    }
}
