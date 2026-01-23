namespace SellerInventory.Shared.Contracts.Product;

public record ImportResultResponse(
    string ProductName,
    bool Success,
    string Message
);
