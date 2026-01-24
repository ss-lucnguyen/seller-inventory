using SellerInventory.Domain.Enums;

namespace SellerInventory.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;

    // Store relationship (nullable for SystemAdmin users)
    public Guid? StoreId { get; set; }
    public Store? Store { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
