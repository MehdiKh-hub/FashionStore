using FashionStore.Web.Data;
using FashionStore.Web.Models.DTOs;
using FashionStore.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.Web.Services;

public class StoreService
{
    private readonly FashionStoreDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StoreService> _logger;

    public StoreService(FashionStoreDbContext context, UserManager<ApplicationUser> userManager, ILogger<StoreService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<List<ProductItemDto>> GetProductsAsync(string? search, Guid? categoryId)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => !p.IsDeleted && p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            query = query.Where(p => p.Name.Contains(normalized) || p.Description!.Contains(normalized));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        return products.Select(p => new ProductItemDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            CategoryName = p.Category.Name,
            Description = p.Description,
            StartingPrice = p.Variants.Where(v => v.IsActive).Select(v => v.Price).DefaultIfEmpty(0).Min(),
            IsActive = p.IsActive,
            MainImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl ?? string.Empty,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

        if (product is null)
        {
            return null;
        }

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            CategoryName = product.Category.Name,
            CategoryId = product.CategoryId,
            Description = product.Description,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Variants = product.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                SKU = v.SKU,
                Price = v.Price,
                StockQuantity = v.StockQuantity,
                IsActive = v.IsActive
            }).ToList(),
            Images = product.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary
            }).ToList()
        };
    }

    public async Task<Product> CreateProductAsync(CreateProductRequestDto dto)
    {
        if (dto.Variants.Count == 0)
        {
            throw new InvalidOperationException("At least one product variant is required.");
        }

        var category = await _context.Categories.FindAsync(dto.CategoryId)
            ?? throw new InvalidOperationException("Category not found.");

        var normalizedName = dto.Name.Trim();
        var slug = GenerateSlug(normalizedName);

        var product = new Product
        {
            Name = normalizedName,
            Slug = slug,
            CategoryId = dto.CategoryId,
            Category = category,
            Description = dto.Description,
            IsActive = dto.IsActive,
            Variants = new List<ProductVariant>(),
            Images = new List<ProductImage>()
        };

        foreach (var variantDto in dto.Variants)
        {
            product.Variants.Add(new ProductVariant
            {
                Size = variantDto.Size.Trim(),
                Color = variantDto.Color.Trim(),
                SKU = variantDto.SKU.Trim(),
                Price = variantDto.Price,
                StockQuantity = variantDto.StockQuantity,
                IsActive = variantDto.IsActive,
                Product = product
            });
        }

        foreach (var imageDto in dto.Images)
        {
            product.Images.Add(new ProductImage
            {
                ImageUrl = imageDto.ImageUrl.Trim(),
                IsPrimary = imageDto.IsPrimary,
                Product = product
            });
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(ci => ci.ProductVariant)
            .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            return new CartDto { Id = Guid.Empty, Items = new List<CartItemDto>() };
        }

        var items = cart.Items.Select(ci => new CartItemDto
        {
            Id = ci.Id,
            ProductVariantId = ci.ProductVariantId,
            ProductName = ci.ProductVariant.Product.Name,
            Size = ci.ProductVariant.Size,
            Color = ci.ProductVariant.Color,
            ImageUrl = ci.ProductVariant.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? ci.ProductVariant.Product.Images.FirstOrDefault()?.ImageUrl ?? string.Empty,
            UnitPrice = ci.ProductVariant.Price,
            Quantity = ci.Quantity
        }).ToList();

        return new CartDto { Id = cart.Id, Items = items };
    }

    public async Task<CartDto> AddOrUpdateCartItemAsync(Guid userId, AddCartItemRequestDto dto)
    {
        if (dto.ProductVariantId == Guid.Empty)
        {
            throw new InvalidOperationException("A valid product variant is required.");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId && v.IsActive);

        if (variant is null)
        {
            throw new InvalidOperationException("Product variant not found or inactive.");
        }

        if (dto.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        if (variant.StockQuantity < dto.Quantity)
        {
            throw new InvalidOperationException($"Only {variant.StockQuantity} item(s) available for this variant.");
        }

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == dto.ProductVariantId);

        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + dto.Quantity;
            if (newQuantity > variant.StockQuantity)
            {
                throw new InvalidOperationException($"Only {variant.StockQuantity} item(s) available in stock.");
            }

            existingItem.Quantity = newQuantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = dto.ProductVariantId,
                Quantity = dto.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            throw new InvalidOperationException("Cart not found.");
        }

        var item = cart.Items.FirstOrDefault(ci => ci.Id == cartItemId);
        if (item is null)
        {
            throw new InvalidOperationException("Cart item not found.");
        }

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequestDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == dto.AddressId && a.UserId == userId)
            ?? throw new InvalidOperationException("Address not found for this user.");

        decimal subtotal = 0m;
        foreach (var item in cart.Items)
        {
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);
            if (variant is null || !variant.IsActive)
            {
                throw new InvalidOperationException($"Variant {item.ProductVariantId} is unavailable.");
            }

            if (item.Quantity > variant.StockQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock for {variant.Product.Name} ({variant.Size}/{variant.Color}).");
            }

            subtotal += variant.Price * item.Quantity;
        }

        var shippingFee = subtotal > 0 ? 15m : 0m;
        var total = subtotal + shippingFee;
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";

        var order = new Order
        {
            UserId = userId,
            AddressId = dto.AddressId,
            OrderNumber = orderNumber,
            Status = "Pending",
            SubTotal = subtotal,
            ShippingFee = shippingFee,
            Total = total,
            Notes = dto.Notes,
            Items = new List<OrderItem>()
        };

        foreach (var item in cart.Items)
        {
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);
            if (variant is null)
            {
                throw new InvalidOperationException("Product variant could not be loaded.");
            }

            var orderItem = new OrderItem
            {
                Order = order,
                ProductVariantId = item.ProductVariantId,
                ProductName = variant.Product.Name,
                Size = variant.Size,
                Color = variant.Color,
                Quantity = item.Quantity,
                UnitPrice = variant.Price,
                TotalPrice = variant.Price * item.Quantity
            };

            order.Items.Add(orderItem);

            variant.StockQuantity -= item.Quantity;
        }

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            SubTotal = order.SubTotal,
            ShippingFee = order.ShippingFee,
            Total = order.Total,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductName = oi.ProductName,
                Size = oi.Size,
                Color = oi.Color,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }

    public async Task<List<OrderDto>> GetOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status,
            SubTotal = o.SubTotal,
            ShippingFee = o.ShippingFee,
            Total = o.Total,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductName = oi.ProductName,
                Size = oi.Size,
                Color = oi.Color,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        }).ToList();
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Addresses = user.Addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                Street = a.Street,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country
            }).ToList()
        };
    }

    public async Task<AddressDto> AddAddressAsync(Guid userId, AddressRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var address = new Address
        {
            UserId = userId,
            Street = dto.Street.Trim(),
            City = dto.City.Trim(),
            State = dto.State.Trim(),
            PostalCode = dto.PostalCode.Trim(),
            Country = dto.Country.Trim()
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return new AddressDto
        {
            Id = address.Id,
            Street = address.Street,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }

    private static string GenerateSlug(string value)
    {
        var normalized = value.Trim();
        return normalized.ToLowerInvariant().Replace(" ", "-");
    }
}
