using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SellerInventer.Application.Interfaces;
using SellerInventer.Domain.Entities;
using SellerInventer.Domain.Enums;

namespace SellerInventer.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@sellerinventer.com",
                PasswordHash = passwordHasher.Hash("Admin@123"),
                FullName = "System Administrator",
                Role = UserRole.Admin,
                IsActive = true
            };

            var managerUser = new User
            {
                Username = "manager",
                Email = "manager@sellerinventer.com",
                PasswordHash = passwordHasher.Hash("Manager@123"),
                FullName = "Store Manager",
                Role = UserRole.Manager,
                IsActive = true
            };

            var staffUser = new User
            {
                Username = "staff",
                Email = "staff@sellerinventer.com",
                PasswordHash = passwordHasher.Hash("Staff@123"),
                FullName = "Staff User",
                Role = UserRole.Staff,
                IsActive = true
            };

            await context.Users.AddRangeAsync(adminUser, managerUser, staffUser);
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Electronics", Description = "Electronic devices and accessories" },
                new() { Name = "Clothing", Description = "Apparel and fashion items" },
                new() { Name = "Food & Beverages", Description = "Food items and drinks" },
                new() { Name = "Home & Garden", Description = "Home improvement and garden supplies" }
            };

            await context.Categories.AddRangeAsync(categories);
        }

        await context.SaveChangesAsync();

        if (!await context.Products.AnyAsync())
        {
            var electronicsCategory = await context.Categories.FirstAsync(c => c.Name == "Electronics");
            var clothingCategory = await context.Categories.FirstAsync(c => c.Name == "Clothing");

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
                    CategoryId = electronicsCategory.Id
                },
                new()
                {
                    Name = "USB-C Hub",
                    Description = "7-in-1 USB-C Hub",
                    SKU = "ELEC-002",
                    CostPrice = 25.00m,
                    SellPrice = 49.99m,
                    StockQuantity = 50,
                    CategoryId = electronicsCategory.Id
                },
                new()
                {
                    Name = "T-Shirt",
                    Description = "Cotton T-Shirt",
                    SKU = "CLTH-001",
                    CostPrice = 8.00m,
                    SellPrice = 19.99m,
                    StockQuantity = 200,
                    CategoryId = clothingCategory.Id
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
