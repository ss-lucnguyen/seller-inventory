namespace SellerInventer.Application.DTOs.Product;

public record CreateProductDto(
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    int StockQuantity,
    Guid CategoryId
);
