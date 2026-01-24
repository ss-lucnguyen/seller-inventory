using SellerInventory.Domain.Interfaces;

namespace SellerInventory.Domain.Entities;

public class Category : BaseEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Store relationship (multi-tenant)
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
