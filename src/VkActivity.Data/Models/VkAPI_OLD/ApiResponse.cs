using System.Text.Json.Serialization;

namespace Home.Data.Models.VkAPI;

public class ApiResponse
{
    public List<ApiUser>? this[int index] => Users;

    [JsonPropertyName("response")]
    public List<ApiUser>? Users { get; set; }
}
