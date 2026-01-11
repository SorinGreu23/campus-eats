using CampusEats.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

public static class MenuItemImageUpdater
{
    public static async Task UpdateMenuItemImages(CampusDbContext context)
    {
        var imageUpdates = new Dictionary<string, string>
        {
            // Existing items
            { "Classic Cheeseburger", "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&h=400&fit=crop" },
            { "Grilled Chicken Wrap", "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=400&h=400&fit=crop" },
            { "Veggie Salad Bowl", "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=400&fit=crop" },
            { "Spicy Ramen", "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=400&fit=crop" },
            { "Spicy Ramen modified", "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=400&fit=crop" },
            { "Chocolate Brownie", "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=400&h=400&fit=crop" },
            
            // New items
            { "Veggie Burrito", "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=400&h=400&fit=crop&q=80" },
            { "Veggie Buritto", "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=400&h=400&fit=crop&q=80" }, // Handle typo
            { "Pancakes", "https://images.unsplash.com/photo-1506084868230-bb9d95c24759?w=400&h=400&fit=crop" },
            { "Pancake", "https://images.unsplash.com/photo-1506084868230-bb9d95c24759?w=400&h=400&fit=crop" },
            { "Breakfast Pancakes", "https://images.unsplash.com/photo-1506084868230-bb9d95c24759?w=400&h=400&fit=crop" },
            { "Italian Burger", "https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=400&h=400&fit=crop" },
            
            // Common menu items that might exist
            { "Caesar Salad", "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=400&h=400&fit=crop" },
            { "Margherita Pizza", "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=400&fit=crop" },
            { "Pepperoni Pizza", "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=400&h=400&fit=crop" },
            { "French Fries", "https://images.unsplash.com/photo-1630384082177-dd91bc6d6235?w=400&h=400&fit=crop" },
            { "Chicken Tenders", "https://images.unsplash.com/photo-1562967914-608f82629710?w=400&h=400&fit=crop" },
            { "Fish and Chips", "https://images.unsplash.com/photo-1579208575657-c595a05383b7?w=400&h=400&fit=crop" },
            { "Tacos", "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400&h=400&fit=crop" },
            { "Sushi", "https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=400&fit=crop" },
            { "Ice Cream", "https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=400&h=400&fit=crop" },
            { "Smoothie", "https://images.unsplash.com/photo-1505252585461-04db1eb84625?w=400&h=400&fit=crop" }
        };

        // Load all menu items once to avoid N+1 query problem
        var itemNames = imageUpdates.Keys.ToList();
        var menuItems = await context.MenuItems
            .Where(m => itemNames.Contains(m.Name))
            .ToListAsync();

        // Update items in memory
        foreach (var menuItem in menuItems)
        {
            if (imageUpdates.TryGetValue(menuItem.Name, out var imageUrl) && 
                string.IsNullOrEmpty(menuItem.ImageUrl))
            {
                menuItem.ImageUrl = imageUrl;
                menuItem.UpdatedAt = DateTimeOffset.UtcNow;
                Console.WriteLine($"Updated image for: {menuItem.Name}");
            }
        }

        if (menuItems.Count > 0)
        {
            await context.SaveChangesAsync();
        }
    }
}
