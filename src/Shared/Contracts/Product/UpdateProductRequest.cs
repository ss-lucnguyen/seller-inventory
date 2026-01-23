namespace SellerInventory.Shared.Contracts.Product;

public record UpdateProductRequest(
    string Name,
    string? Description,
    string? SKU,
    decimal CostPrice,
    decimal SellPrice,
    int StockQuantity,
    Guid CategoryId,
    bool IsActive,
    string? ImageUrl = null
);
