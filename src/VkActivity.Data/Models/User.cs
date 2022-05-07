namespace VkActivity.Data.Models;

/// <summary>Vk user (DB)</summary>
public partial class User
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RawData { get; set; }
    public string? RawDataHistory { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime InsertDate { get; set; }
}
