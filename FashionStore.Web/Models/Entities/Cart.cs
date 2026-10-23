using System.ComponentModel.DataAnnotations;

namespace FashionStore.Web.Models.Entities;

public class Cart
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
