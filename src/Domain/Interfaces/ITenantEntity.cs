namespace SellerInventory.Domain.Interfaces;

public interface ITenantEntity
{
    Guid StoreId { get; set; }
}
