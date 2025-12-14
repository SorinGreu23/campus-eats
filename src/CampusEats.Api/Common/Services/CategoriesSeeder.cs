using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

public static class CategoriesSeeder
{
    public static async Task SeedCategories(CampusDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Burgers",
                DisplayOrder = 1,
                IsActive = true,
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Wraps",
                DisplayOrder = 2,
                IsActive = true,
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Salads",
                DisplayOrder = 3,
                IsActive = true,
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Noodles",
                DisplayOrder = 4,
                IsActive = true,
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Desserts",
                DisplayOrder = 5,
                IsActive = true,
            },
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}
