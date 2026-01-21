namespace SellerInventer.Shared.Contracts.Product;

public record UpdateProductRequest(
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    bool IsActive
);
