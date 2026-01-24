namespace SellerInventory.Shared.Contracts.Store;

public record StoreResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Location,
    string? Address,
    string? Industry,
    string? LogoUrl,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? Currency,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
