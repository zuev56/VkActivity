using System.Text.Json.Serialization;

namespace VkActivity.Worker.Models.VkApi;

public sealed class UsersApiResponse
{
    [JsonPropertyName("response")]
    public List<VkApiUser>? Users { get; init; }
}
