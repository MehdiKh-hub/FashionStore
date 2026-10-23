using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FashionStore.Web.Models.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Bio { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public Cart? Cart { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
