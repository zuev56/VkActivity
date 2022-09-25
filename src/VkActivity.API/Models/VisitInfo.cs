using VkActivity.Data.Models;

namespace VkActivity.Api.Models
{
    public sealed class VisitInfo
    {
        public Platform Platform { get; set; }
        public int Count { get; set; }
        public TimeSpan Time { get; set; }
    }
}
