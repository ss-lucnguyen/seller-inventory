using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Interfaces;

public interface ITenantContext
{
    Guid? CurrentStoreId { get; }
    Guid? CurrentUserId { get; }
    UserRole CurrentUserRole { get; }
    bool IsSystemAdmin { get; }
    bool HasStoreAccess { get; }
    void SetTenant(Guid? storeId, Guid userId, UserRole role);
}
