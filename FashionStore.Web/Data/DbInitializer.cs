using FashionStore.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FashionStoreDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Men", Description = "Men fashion" },
                new Category { Name = "Women", Description = "Women fashion" },
                new Category { Name = "Kids", Description = "Kids fashion" },
                new Category { Name = "Accessories", Description = "Accessories" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            }

            var admin = new ApplicationUser
            {
                UserName = "admin@fashionstore.com",
                Email = "admin@fashionstore.com",
                FirstName = "Admin",
                LastName = "Store",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        if (!await context.Products.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var men = categories.First(c => c.Name == "Men");
            var women = categories.First(c => c.Name == "Women");

            var product1 = new Product
            {
                Name = "Classic Cotton Tee",
                Slug = "classic-cotton-tee",
                Description = "Comfortable cotton tee for everyday wear.",
                CategoryId = men.Id,
                IsActive = true,
                Variants =
                [
                    new ProductVariant { Size = "S", Color = "White", SKU = "M-CT-001-S-WH", Price = 49.99m, StockQuantity = 10, IsActive = true },
                    new ProductVariant { Size = "M", Color = "Black", SKU = "M-CT-001-M-BK", Price = 49.99m, StockQuantity = 12, IsActive = true }
                ],
                Images =
                [
                    new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=crop&w=800&q=80", IsPrimary = true },
                    new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=crop&w=800&q=80", IsPrimary = false }
                ]
            };

            var product2 = new Product
            {
                Name = "Elegant Evening Dress",
                Slug = "elegant-evening-dress",
                Description = "A stylish dress for special occasions.",
                CategoryId = women.Id,
                IsActive = true,
                Variants =
                [
                    new ProductVariant { Size = "M", Color = "Red", SKU = "W-ED-002-M-RE", Price = 129.99m, StockQuantity = 8, IsActive = true },
                    new ProductVariant { Size = "L", Color = "Navy", SKU = "W-ED-002-L-NA", Price = 129.99m, StockQuantity = 6, IsActive = true }
                ],
                Images =
                [
                    new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1496747611176-843222e1e57c?auto=format&fit=crop&w=800&q=80", IsPrimary = true }
                ]
            };

            context.Products.AddRange(product1, product2);
            await context.SaveChangesAsync();
        }
    }
}
