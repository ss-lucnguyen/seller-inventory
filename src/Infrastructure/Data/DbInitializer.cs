using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        // Create default store for development
        Store? demoStore = null;
        if (!await context.Stores.AnyAsync())
        {
            demoStore = new Store
            {
                Name = "Demo Store",
                Slug = "demo-store",
                Location = "New York",
                Address = "123 Demo Street, NY 10001",
                Industry = "Retail",
                Currency = "USD",
                IsActive = true,
                SubscriptionStatus = SubscriptionStatus.Active,
                SubscriptionExpiresAt = DateTime.UtcNow.AddYears(1),
                ContactEmail = "contact@demostore.com",
                ContactPhone = "+1-555-123-4567"
            };

            await context.Stores.AddAsync(demoStore);
            await context.SaveChangesAsync();
        }
        else
        {
            demoStore = await context.Stores.FirstAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            // SystemAdmin - not tied to any store
            var systemAdmin = new User
            {
                Username = "sysadmin",
                Email = "sysadmin@sellerinventory.com",
                PasswordHash = passwordHasher.Hash("SysAdmin@123"),
                FullName = "System Administrator",
                Role = UserRole.SystemAdmin,
                StoreId = null, // SystemAdmin has no store
                IsActive = true
            };

            // Manager - tied to demo store
            var managerUser = new User
            {
                Username = "manager",
                Email = "manager@demostore.com",
                PasswordHash = passwordHasher.Hash("Manager@123"),
                FullName = "Store Manager",
                Role = UserRole.Manager,
                StoreId = demoStore.Id,
                IsActive = true
            };

            // Staff - tied to demo store
            var staffUser = new User
            {
                Username = "staff",
                Email = "staff@demostore.com",
                PasswordHash = passwordHasher.Hash("Staff@123"),
                FullName = "Staff User",
                Role = UserRole.Staff,
                StoreId = demoStore.Id,
                IsActive = true
            };

            await context.Users.AddRangeAsync(systemAdmin, managerUser, staffUser);
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Electronics", Description = "Electronic devices and accessories", StoreId = demoStore.Id },
                new() { Name = "Clothing", Description = "Apparel and fashion items", StoreId = demoStore.Id },
                new() { Name = "Food & Beverages", Description = "Food items and drinks", StoreId = demoStore.Id },
                new() { Name = "Home & Garden", Description = "Home improvement and garden supplies", StoreId = demoStore.Id }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        if (!await context.Products.AnyAsync())
        {
            var electronicsCategory = await context.Categories.FirstAsync(c => c.Name == "Electronics" && c.StoreId == demoStore.Id);
            var clothingCategory = await context.Categories.FirstAsync(c => c.Name == "Clothing" && c.StoreId == demoStore.Id);

            var products = new List<Product>
            {
                new()
                {
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse",
                    SKU = "ELEC-001",
                    CostPrice = 15.00m,
                    SellPrice = 29.99m,
                    StockQuantity = 100,
                    CategoryId = electronicsCategory.Id,
                    StoreId = demoStore.Id
                },
                new()
                {
                    Name = "USB-C Hub",
                    Description = "7-in-1 USB-C Hub",
                    SKU = "ELEC-002",
                    CostPrice = 25.00m,
                    SellPrice = 49.99m,
                    StockQuantity = 50,
                    CategoryId = electronicsCategory.Id,
                    StoreId = demoStore.Id
                },
                new()
                {
                    Name = "T-Shirt",
                    Description = "Cotton T-Shirt",
                    SKU = "CLTH-001",
                    CostPrice = 8.00m,
                    SellPrice = 19.99m,
                    StockQuantity = 200,
                    CategoryId = clothingCategory.Id,
                    StoreId = demoStore.Id
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
