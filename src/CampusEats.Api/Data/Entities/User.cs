using System;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Data.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
}
