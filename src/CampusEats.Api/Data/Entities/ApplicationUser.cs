using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string UserId { get; set; }

    public User? User { get; set; }
}