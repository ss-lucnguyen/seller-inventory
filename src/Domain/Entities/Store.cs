using SellerInventory.Domain.Enums;

namespace SellerInventory.Domain.Entities;

public class Store : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Address { get; set; }
    public string? Industry { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trial;
    public DateTime? SubscriptionExpiresAt { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
