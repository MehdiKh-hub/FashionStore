using System.ComponentModel.DataAnnotations;

namespace FashionStore.Web.Models.DTOs;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ProductItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal StartingPrice { get; set; }
    public bool IsActive { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
}

public class CreateProductRequestDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }

    [StringLength(1500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [MinLength(1)]
    public List<CreateProductVariantRequestDto> Variants { get; set; } = new();

    public List<CreateProductImageRequestDto> Images { get; set; } = new();
}

public class UpdateProductRequestDto
{
    [StringLength(150)]
    public string? Name { get; set; }

    public Guid? CategoryId { get; set; }

    [StringLength(1500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

public class CreateProductVariantRequestDto
{
    [Required]
    [StringLength(30)]
    public string Size { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Color { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string SKU { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(0, 10000)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreateProductImageRequestDto
{
    [Required]
    [Url]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}
