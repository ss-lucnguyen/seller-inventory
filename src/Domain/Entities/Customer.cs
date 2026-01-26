using SellerInventory.Domain.Enums;
using SellerInventory.Domain.Interfaces;

namespace SellerInventory.Domain.Entities;

public class Customer : BaseEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Unknown;
    public string? Mobile { get; set; }
    public string? AccountNumber { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    // Store relationship (multi-tenant)
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
