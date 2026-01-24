namespace SellerInventory.Application.DTOs.Product;

public record CreateProductDto(
    string Name,
    string? Description,
    string? SKU,
    decimal CostPrice,
    decimal SellPrice,
    int StockQuantity,
    Guid CategoryId,
    string? ImageUrl = null
);
