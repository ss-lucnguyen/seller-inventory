namespace SellerInventory.Application.DTOs.Product;

public record ImportProductDto(
    string Name,
    string? Description,
    string? SKU,
    decimal CostPrice,
    decimal SellPrice,
    int StockQuantity,
    string CategoryName
);
