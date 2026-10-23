using System.ComponentModel.DataAnnotations;

namespace FashionStore.Web.Models.Entities;

public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    [Required]
    public Guid AddressId { get; set; }

    public Address Address { get; set; } = null!;

    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Pending";

    [Required]
    [Range(0, 100000000)]
    public decimal SubTotal { get; set; }

    [Required]
    [Range(0, 100000000)]
    public decimal ShippingFee { get; set; }

    [Required]
    [Range(0, 100000000)]
    public decimal Total { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
