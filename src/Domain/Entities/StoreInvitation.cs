using SellerInventory.Domain.Enums;

namespace SellerInventory.Domain.Entities;

public class StoreInvitation : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public Guid InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;
}
