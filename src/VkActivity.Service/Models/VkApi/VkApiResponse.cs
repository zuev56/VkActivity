using System.Text.Json.Serialization;

namespace VkActivity.Service.Models.VkApi;

public sealed class VkApiResponse
{
    [JsonPropertyName("response")]
    public List<VkApiUser>? Users { get; init; }
}
