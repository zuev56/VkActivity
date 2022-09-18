using System.Text.Json.Serialization;

namespace VkActivity.Data.Models
{
    public class DbInfo
    {
        [JsonPropertyName("table")]
        public string Table { get; set; } = null!;
        [JsonPropertyName("rows")]
        public int Rows { get; set; }
        [JsonPropertyName("size")]
        public string Size { get; set; } = null!;
    }
}
