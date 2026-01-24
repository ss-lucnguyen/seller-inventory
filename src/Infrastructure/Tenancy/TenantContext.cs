using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Infrastructure.Tenancy;

public class TenantContext : ITenantContext
{
    private Guid? _storeId;
    private Guid? _userId;
    private UserRole _role = UserRole.Staff;

    public Guid? CurrentStoreId => _storeId;
    public Guid? CurrentUserId => _userId;
    public UserRole CurrentUserRole => _role;
    public bool IsSystemAdmin => _role == UserRole.SystemAdmin;
    public bool HasStoreAccess => _storeId.HasValue || IsSystemAdmin;

    public void SetTenant(Guid? storeId, Guid userId, UserRole role)
    {
        _storeId = storeId;
        _userId = userId;
        _role = role;
    }
}
