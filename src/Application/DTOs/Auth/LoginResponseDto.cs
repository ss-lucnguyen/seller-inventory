namespace SellerInventory.Application.DTOs.Auth;

public record LoginResponseDto(
    string Token,
    string Username,
    string FullName,
    string Role,
    Guid? StoreId,
    string? StoreName,
    DateTime ExpiresAt
);
