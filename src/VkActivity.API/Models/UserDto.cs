using VkActivity.Data.Models;

namespace VkActivity.Api.Models
{
    public sealed class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
