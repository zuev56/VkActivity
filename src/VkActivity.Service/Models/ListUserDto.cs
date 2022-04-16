namespace VkActivity.Service.Models;

public class ListUserDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsOnline { get; set; }
    public int ActivitySec { get; internal set; }
}
