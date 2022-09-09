using System.Text.Json.Serialization;

namespace VkActivity.Worker.Models.VkApi;

public sealed class FriendsApiResponse
{
    [JsonPropertyName("response")]
    public ResponseData? Data { get; init; }

    public class ResponseData
    {
        [JsonPropertyName("count")]
        public int Count { get; init; }

        [JsonPropertyName("items")]
        public int[]? FriendIds { get; init; }
    }
}
