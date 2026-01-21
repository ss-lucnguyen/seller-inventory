namespace SellerInventer.Application.DTOs.Product;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string? SKU,
    decimal CostPrice,
    decimal SellPrice,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
