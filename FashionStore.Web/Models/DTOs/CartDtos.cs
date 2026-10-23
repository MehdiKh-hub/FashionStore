using System.ComponentModel.DataAnnotations;

namespace FashionStore.Web.Models.DTOs;

public class AddCartItemRequestDto
{
    [Required]
    public Guid ProductVariantId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}

public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.TotalPrice);
}
