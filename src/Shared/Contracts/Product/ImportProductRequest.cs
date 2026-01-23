namespace SellerInventory.Shared.Contracts.Product;

public record ImportProductRequest(
    string Name,
    string? Description,
    string? SKU,
    decimal CostPrice,
    decimal SellPrice,
    int StockQuantity,
    string CategoryName
);
