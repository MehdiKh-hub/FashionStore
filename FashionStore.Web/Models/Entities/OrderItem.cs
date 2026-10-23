using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionStore.Web.Models.Entities;

public class OrderItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [Required]
    public Guid ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Size { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 1000000)]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 1000000)]
    public decimal TotalPrice { get; set; }
}
