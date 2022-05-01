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
    public int Id { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("online")]
    public int Online { get; init; }

    [JsonPropertyName("last_seen")]
    public VkApiLastSeen? LastSeen { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? RawData { get; init; }

    //public string ToJson() => JsonSerializer.Serialize(this, _jsonSerializerOptions).NormalizeJsonString();

    // TODO: move to mapper
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
