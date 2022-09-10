using System.Text.Json.Serialization;

namespace VkActivity.Common.Models.VkApi;

public sealed class UsersApiResponse
{
    [JsonPropertyName("response")]
    public List<VkApiUser>? Users { get; init; }
}
