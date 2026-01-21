namespace SellerInventer.Shared.Contracts.Product;

public record CreateProductRequest(
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    int StockQuantity,
    Guid CategoryId
);
