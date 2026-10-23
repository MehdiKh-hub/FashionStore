using System.ComponentModel.DataAnnotations;

namespace FashionStore.Web.Models.Entities;

public class CartItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CartId { get; set; }

    public Cart Cart { get; set; } = null!;

    [Required]
    public Guid ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    [Range(1, 100)]
    public int Quantity { get; set; }
}
