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

internal static class MapperConfiguration
{

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static AutoMapper.MapperConfiguration CreateMapperConfiguration()
    {
        return new AutoMapper.MapperConfiguration(config =>
        {
            config.CreateMap<VkApiUser, User>()
                .ForMember(destination => destination.RawData, member => member.MapFrom(i => JsonSerializer.Serialize(i, _jsonSerializerOptions).NormalizeJsonString()))
                .ForMember(destination => destination.InsertDate, member => member.MapFrom(_ => DateTime.UtcNow))
                .ForMember(destination => destination.UpdateDate, member => member.MapFrom(_ => DateTime.UtcNow));

            //config.CreateMap<User, ListUserDto>()
            //    .ForMember(destination => destination.Name, member => member.MapFrom(i => $"{i.FirstName} {i.LastName}"))
            //    .ForMember(destination => destination.IsOnline, member => member.MapFrom(i => false))
            //    .ForMember(destination => destination.ActivitySec, member => member.MapFrom(i => -1));

            config.CreateMap<ActivityListItem, ListUserDto>()
                .ForMember(destination => destination.Id, member => member.MapFrom(i => i.User!.Id))
                .ForMember(destination => destination.Name, member => member.MapFrom(i => $"{i.User!.FirstName} {i.User.LastName}"))
                .ForMember(destination => destination.IsOnline, member => member.MapFrom(i => i.IsOnline))
                .ForMember(destination => destination.ActivitySec, member => member.MapFrom(i => i.ActivitySec));

            config.CreateMap<SimpleActivity, PeriodInfoDto>()
                .ForMember(destination => destination.TimeInSite, member => member.MapFrom(i => i.TimeInSite.ToDayHHmmss()))
                .ForMember(destination => destination.TimeInApp, member => member.MapFrom(i => i.TimeInApp.ToDayHHmmss()))
                .ForMember(destination => destination.FullTime, member => member.MapFrom(i => (i.TimeInSite + i.TimeInApp).ToDayHHmmss()));

            config.CreateMap<DetailedActivity, FullTimeInfoDto>()
                .ForMember(destination => destination.TimeInSite, member => member.MapFrom(i => i.TimeInSite.ToDayHHmmss()))
                .ForMember(destination => destination.TimeInApp, member => member.MapFrom(i => i.TimeInApp.ToDayHHmmss()))
                .ForMember(destination => destination.FullTime, member => member.MapFrom(i => i.FullTime.ToDayHHmmss()))
                .ForMember(destination => destination.AvgDailyTime, member => member.MapFrom(i => i.AvgDailyTime.ToDayHHmmss()));
        });
    }
}
