namespace SellerInventory.Shared.Contracts.Auth;

public record LoginResponse(
    string Token,
    string Username,
    string FullName,
    string Role,
    DateTime ExpiresAt
);
