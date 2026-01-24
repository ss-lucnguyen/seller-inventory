namespace SellerInventory.Shared.Contracts.Store;

public record UpdateStoreRequest(
    string Name,
    string? Location,
    string? Address,
    string? Industry,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? Currency
);
