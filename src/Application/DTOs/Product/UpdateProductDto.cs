namespace SellerInventer.Application.DTOs.Product;

public record UpdateProductDto(
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    bool IsActive
);
