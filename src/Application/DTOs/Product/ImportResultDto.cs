namespace SellerInventory.Application.DTOs.Product;

public record ImportResultDto(
    string ProductName,
    bool Success,
    string Message
);
