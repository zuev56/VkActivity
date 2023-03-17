using System;

namespace VkActivity.Data.Models;

/// <summary>Vk user (DB)</summary>
public partial class User : IEquatable<User?>
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string RawData { get; set; } = null!;
    public string? RawDataHistory { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime InsertDate { get; set; }
    public Status Status { get; set; }

    public string GetFullName() => $"{FirstName} {LastName}";

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
