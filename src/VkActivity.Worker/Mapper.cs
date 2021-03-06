using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using VkActivity.Data.Models;
using VkActivity.Worker.Models;
using VkActivity.Worker.Models.Dto;
using VkActivity.Worker.Models.VkApi;
using Zs.Common.Extensions;

namespace VkActivity.Worker;

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

        //Просто удалить из словаря ненужные поля перед сериализацией
        var json = JsonSerializer.Serialize(apiVkUser, _jsonSerializerOptions);

        return new User
        {
            Id = apiVkUser.Id,
            FirstName = apiVkUser.FirstName,
            LastName = apiVkUser.LastName,
            RawData = json,
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
