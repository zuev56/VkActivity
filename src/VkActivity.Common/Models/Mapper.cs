using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using VkActivity.Common.Models.VkApi;
using DbUser = VkActivity.Data.Models.User;
using State = VkActivity.Data.Models.State;

namespace VkActivity.Common;

public static class Mapper
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static DbUser ToUser(VkApiUser apiVkUser)
    {
        ArgumentNullException.ThrowIfNull(nameof(apiVkUser));

        //Просто удалить из словаря ненужные поля перед сериализацией
        var json = JsonSerializer.Serialize(apiVkUser, _jsonSerializerOptions);

        return new DbUser
        {
            Id = apiVkUser.Id,
            FirstName = apiVkUser.FirstName,
            LastName = apiVkUser.LastName,
            State = apiVkUser.Deactivated switch
            {
                "deleted" => State.Deleted,
                "banned" => State.Banned,
                _ => State.Active
            },
            RawData = json,
            RawDataHistory = null,
            InsertDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };
    }
}
