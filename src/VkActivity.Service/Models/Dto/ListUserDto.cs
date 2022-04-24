namespace VkActivity.Service.Models.Dto;

public class ListUserDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsOnline { get; set; }
    public int ActivitySec { get; internal set; }
}
