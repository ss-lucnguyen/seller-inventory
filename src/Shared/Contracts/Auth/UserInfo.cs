namespace SellerInventory.Shared.Contracts.Auth;

public record UserInfo(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    bool IsActive
);
