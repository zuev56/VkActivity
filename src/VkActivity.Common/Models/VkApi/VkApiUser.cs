using System.Text.Json;
using System.Text.Json.Serialization;

namespace VkActivity.Common.Models.VkApi;

public sealed class VkApiUser
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("online")]
    public int IsOnline { get; init; }

    // Надо парсить только когда это свойство есть
    [JsonPropertyName("deactivated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Deactivated { get; init; }

    [JsonPropertyName("deleted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Deleted { get; init; }

    [JsonPropertyName("last_seen")]
    public VkApiLastSeen? LastSeen { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? RawData { get; init; }

}
