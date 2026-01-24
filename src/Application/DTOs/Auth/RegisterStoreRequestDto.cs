namespace SellerInventory.Application.DTOs.Auth;

public record RegisterStoreRequestDto(
    string StoreName,
    string StoreSlug,
    string? Location,
    string? Address,
    string? Industry,
    string? Currency,
    string OwnerUsername,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerFullName
);
