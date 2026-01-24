namespace SellerInventory.Application.DTOs.Store;

public record UpdateStoreDto(
    string Name,
    string? Location,
    string? Address,
    string? Industry,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? Currency
);
