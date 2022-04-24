using System.Text.Json.Serialization;

namespace VkActivity.Service.Models.VkApi;

public class VkApiLastSeen
{
    [JsonPropertyName("time")]
    public int Time { get; set; }
}
