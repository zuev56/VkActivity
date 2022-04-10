using System.Text.Json.Serialization;

namespace VkActivity.Service.Models;

public class ApiLastSeen
{
    [JsonPropertyName("time")]
    public int Time { get; set; }
}
