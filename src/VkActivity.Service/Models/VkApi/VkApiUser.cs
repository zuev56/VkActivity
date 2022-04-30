using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using VkActivity.Data.Models;
using Zs.Common.Extensions;

namespace VkActivity.Service.Models.VkApi;

public sealed class VkApiUser
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("online")]
    public int Online { get; set; }

    [JsonPropertyName("online_mobile")]
    public int OnlineMobile { get; set; }

    [JsonPropertyName("online_app")]
    public int OnlineApp { get; set; }

    [JsonPropertyName("last_seen")]
    public VkApiLastSeen? LastSeen { get; set; }
    public DateTime LastSeenFromUnixTime => LastSeen is null
        ? DateTime.MinValue
        : LastSeen.UnixTime.FromUnixEpoch();

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? RawData { get; set; }


    public static explicit operator User(VkApiUser apiVkUser)
    {
        return new User()
        {
            Id = apiVkUser.Id,
            FirstName = apiVkUser.FirstName,
            LastName = apiVkUser.LastName,
            RawData = JsonSerializer.Serialize(apiVkUser, _jsonSerializerOptions).NormalizeJsonString(),
            InsertDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };
    }
}
