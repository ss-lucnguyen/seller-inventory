namespace SellerInventer.Application.DTOs.Product;

public record UpdateProductDto(
    string Name,
    string? Description,
    string? SKU,
    decimal CostPrice,
    decimal SellPrice,
    int StockQuantity,
    Guid CategoryId,
    bool IsActive
);
