namespace SellerInventer.Shared.Contracts.Product;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
