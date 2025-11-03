using System;

namespace CampusEats.Api.Data.Entities;

public class Notification : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public Guid? OrderId { get; set; }
}
