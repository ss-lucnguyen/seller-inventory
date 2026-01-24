namespace SellerInventory.Shared.Contracts.Auth;

public record LoginResponse(
    string Token,
    string Username,
    string FullName,
    string Role,
    Guid? StoreId,
    string? StoreName,
    DateTime ExpiresAt
);
