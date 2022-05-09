namespace VkActivity.Data.Models;

/// <summary>Vk user (DB)</summary>
public partial class User : IEquatable<User?>
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RawData { get; set; }
    public string? RawDataHistory { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime InsertDate { get; set; }

    public override bool Equals(object? obj)
    {
        return Equals(obj as User);
    }

    public bool Equals(User? other)
    {
        return other != null
            && Id == other.Id
            && FirstName == other.FirstName
            && LastName == other.LastName
            && RawData == other.RawData;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FirstName, LastName, RawData);
    }
}
