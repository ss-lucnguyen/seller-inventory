namespace SellerInventer.Shared.Contracts.Product;

public record CreateProductRequest(
    string Name,
    string? Description,
    string? SKU,
    decimal CostPrice,
    decimal SellPrice,
    int StockQuantity,
    Guid CategoryId
);
