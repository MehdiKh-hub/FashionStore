using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionStore.Web.Models.Entities;

public class ProductVariant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string Size { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Color { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, 10000)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
