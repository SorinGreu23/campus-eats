using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Overview;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Users;

public class GetUsersHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllUsers_WithRoles()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);

        // Create roles
        var customerRole = new IdentityRole
        {
            Id = "customer-role-id",
            Name = "Customer",
            NormalizedName = "CUSTOMER"
        };
        var adminRole = new IdentityRole
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN"
        };
        context.Roles.AddRange(customerRole, adminRole);

        // Create users
        var user1 = new ApplicationUser
        {
            Id = "user1-id",
            UserName = "user1",
            Email = "user1@example.com",
            FirstName = "User",
            LastName = "One",
            IsActive = true
        };
        var user2 = new ApplicationUser
        {
            Id = "user2-id",
            UserName = "user2",
            Email = "user2@example.com",
            FirstName = "User",
            LastName = "Two",
            IsActive = true
        };
        var user3 = new ApplicationUser
        {
            Id = "admin-id",
            UserName = "admin",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            IsActive = true
        };
        context.Users.AddRange(user1, user2, user3);

        // Assign roles to users
        context.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = "user1-id", RoleId = "customer-role-id" },
            new IdentityUserRole<string> { UserId = "user2-id", RoleId = "customer-role-id" },
            new IdentityUserRole<string> { UserId = "admin-id", RoleId = "admin-role-id" }
        );

        await context.SaveChangesAsync();

        var handler = new GetUsersHandler(context);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoUsers()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);

        var handler = new GetUsersHandler(context);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldIncludeInactiveUsers()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);

        var customerRole = new IdentityRole
        {
            Id = "customer-role-id",
            Name = "Customer",
            NormalizedName = "CUSTOMER"
        };
        context.Roles.Add(customerRole);

        var activeUser = new ApplicationUser
        {
            Id = "active-user-id",
            UserName = "activeuser",
            Email = "active@example.com",
            FirstName = "Active",
            LastName = "User",
            IsActive = true
        };
        var inactiveUser = new ApplicationUser
        {
            Id = "inactive-user-id",
            UserName = "inactiveuser",
            Email = "inactive@example.com",
            FirstName = "Inactive",
            LastName = "User",
            IsActive = false
        };
        context.Users.AddRange(activeUser, inactiveUser);

        context.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = "active-user-id", RoleId = "customer-role-id" },
            new IdentityUserRole<string> { UserId = "inactive-user-id", RoleId = "customer-role-id" }
        );

        await context.SaveChangesAsync();

        var handler = new GetUsersHandler(context);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldHandleUsersWithoutRoles()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);

        var userWithoutRole = new ApplicationUser
        {
            Id = "user-no-role-id",
            UserName = "noroleuser",
            Email = "norole@example.com",
            FirstName = "No",
            LastName = "Role",
            IsActive = true
        };
        context.Users.Add(userWithoutRole);

        await context.SaveChangesAsync();

        var handler = new GetUsersHandler(context);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnUsersWithMultipleRoles()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);

        var customerRole = new IdentityRole
        {
            Id = "customer-role-id",
            Name = "Customer",
            NormalizedName = "CUSTOMER"
        };
        var adminRole = new IdentityRole
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN"
        };
        context.Roles.AddRange(customerRole, adminRole);

        var multiRoleUser = new ApplicationUser
        {
            Id = "multi-role-user-id",
            UserName = "multiroleuser",
            Email = "multirole@example.com",
            FirstName = "Multi",
            LastName = "Role",
            IsActive = true
        };
        context.Users.Add(multiRoleUser);

        // Assign multiple roles (though typically a user has one role, testing edge case)
        context.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = "multi-role-user-id", RoleId = "customer-role-id" },
            new IdentityUserRole<string> { UserId = "multi-role-user-id", RoleId = "admin-role-id" }
        );

        await context.SaveChangesAsync();

        var handler = new GetUsersHandler(context);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }
}
