namespace SellerInventory.Application.DTOs.Auth;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    Guid? StoreId,
    string? StoreName
);
