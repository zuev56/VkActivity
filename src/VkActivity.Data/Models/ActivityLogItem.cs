﻿namespace VkActivity.Data.Models;

/// <summary>Vk activity item (DB)</summary>
public partial class ActivityLogItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool? IsOnline { get; set; }
    public DateTime InsertDate { get; set; }
    public int? OnlineApp { get; set; }
    public bool IsOnlineMobile { get; set; }
    public int LastSeen { get; set; }
    public User? User { get; set; }
}
