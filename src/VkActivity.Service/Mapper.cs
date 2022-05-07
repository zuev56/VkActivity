using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using VkActivity.Data.Models;
using VkActivity.Service.Models;
using VkActivity.Service.Models.Dto;
using VkActivity.Service.Models.VkApi;
using Zs.Common.Extensions;

namespace VkActivity.Service;

public static class Mapper
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static User ToUser(VkApiUser apiVkUser)
    {
        ArgumentNullException.ThrowIfNull(nameof(apiVkUser));

        return new User
        {
            Id = apiVkUser.Id,
            FirstName = apiVkUser.FirstName,
            LastName = apiVkUser.LastName,
            RawData = JsonSerializer.Serialize(apiVkUser, _jsonSerializerOptions).NormalizeJsonString(),
            RawDataHistory = null,
            InsertDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };
    }

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
            FullTime = (simpleActivity.TimeInSite + simpleActivity.TimeInApp).ToDayHHmmss()
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
            FullTime = detailedActivity.FullTime.ToDayHHmmss(),
            AvgDailyTime = detailedActivity.AvgDailyTime.ToDayHHmmss()
        };
    }
}
