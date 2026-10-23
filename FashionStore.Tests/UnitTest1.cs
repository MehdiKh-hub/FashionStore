using FashionStore.Web.Data;
using FashionStore.Web.Models.DTOs;
using FashionStore.Web.Models.Entities;
using FashionStore.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FashionStore.Tests;

public class StoreServiceTests
{
    [Fact]
    public async Task AddAddressAsync_ShouldPersistAddressForUser()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FashionStoreDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddLogging();
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<FashionStoreDbContext>()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "tester@example.com",
            Email = "tester@example.com",
            FirstName = "Tester",
            LastName = "User",
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, "Password123!");
        Assert.True(result.Succeeded);

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FashionStoreDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<StoreService>>();
        var service = new StoreService(context, userManager, logger);

        var dto = new AddressRequestDto
        {
            Street = "123 Test St",
            City = "Sample City",
            State = "Sample State",
            PostalCode = "12345",
            Country = "US"
        };

        var saved = await service.AddAddressAsync(user.Id, dto);

        Assert.Equal("123 Test St", saved.Street);

        var record = await context.Addresses.SingleAsync(a => a.UserId == user.Id);
        Assert.Equal("123 Test St", record.Street);
    }

    [Fact]
    public async Task AddOrUpdateCartItemAsync_ShouldAccumulateQuantityWithoutConcurrencyIssues()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FashionStoreDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddLogging();
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<FashionStoreDbContext>()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FashionStoreDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<StoreService>>();
        var service = new StoreService(context, userManager, logger);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "carttester@example.com",
            Email = "carttester@example.com",
            FirstName = "Cart",
            LastName = "Tester",
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(user, "Password123!");
        Assert.True(createResult.Succeeded);

        var category = new Category { Name = "Test Category", Description = "For tests" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            CategoryId = category.Id,
            Description = "A product for cart tests",
            IsActive = true,
            Variants =
            [
                new ProductVariant
                {
                    Size = "M",
                    Color = "Black",
                    SKU = "TEST-001-M-BLK",
                    Price = 59.99m,
                    StockQuantity = 10,
                    IsActive = true
                }
            ],
            Images =
            [
                new ProductImage { ImageUrl = "https://example.com/test.jpg", IsPrimary = true }
            ]
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = await context.ProductVariants.SingleAsync(v => v.SKU == "TEST-001-M-BLK");

        var first = await service.AddOrUpdateCartItemAsync(user.Id, new AddCartItemRequestDto
        {
            ProductVariantId = variant.Id,
            Quantity = 2
        });

        var second = await service.AddOrUpdateCartItemAsync(user.Id, new AddCartItemRequestDto
        {
            ProductVariantId = variant.Id,
            Quantity = 3
        });

        Assert.Equal(5, second.Items.Single().Quantity);
        Assert.Equal(299.95m, second.Total);
        Assert.Equal(1, await context.CartItems.CountAsync(ci => ci.CartId == first.Id && ci.ProductVariantId == variant.Id));
    }
}