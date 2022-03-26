using System.Text.Json.Serialization;

namespace Home.Data.Models.VkAPI;

public class ApiLastSeen
{
    [JsonPropertyName("time")]
    public int Time { get; set; }
}
