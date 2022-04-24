using System.Text.Json.Serialization;

namespace VkActivity.Service.Models.VkApi;

public class VkApiResponse
{
    public List<VkApiUser>? this[int index] => Users;

    [JsonPropertyName("response")]
    public List<VkApiUser>? Users { get; set; }
}
