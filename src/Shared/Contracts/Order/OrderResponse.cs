namespace SellerInventory.Shared.Contracts.Order;

public record OrderResponse(
    Guid Id,
    string OrderNumber,
    DateTime OrderDate,
    string Status,
    decimal SubTotal,
    decimal Tax,
    decimal Discount,
    decimal Total,
    string? Notes,
    Guid UserId,
    string UserName,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);
