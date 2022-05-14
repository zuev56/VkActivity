using System.Text.Json.Serialization;
using VkActivity.Data.Models;

namespace VkActivity.Worker.Models.VkApi;

// https://dev.vk.com/reference/objects/user#last_seen
public sealed class VkApiLastSeen
{
    [JsonPropertyName("time")]
    public int UnixTime { get; init; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; init; }
}
