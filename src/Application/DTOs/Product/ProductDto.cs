namespace SellerInventory.Application.DTOs.Product;

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
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
