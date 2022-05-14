using System.Text.Json;
using System.Text.Json.Serialization;

namespace VkActivity.Service.Models.VkApi;
public enum State
{
    Active = 0,
    Banned,
    Deleted
}

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
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public State State { get; init; }

    [JsonPropertyName("last_seen")]
    public VkApiLastSeen? LastSeen { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? RawData { get; init; }

}
