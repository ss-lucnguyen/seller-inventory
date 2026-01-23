namespace SellerInventory.Shared.Contracts.Order;

public record CreateOrderRequest(
    IReadOnlyList<CreateOrderItemRequest> Items,
    decimal Tax,
    decimal Discount,
    string? Notes
);

public record CreateOrderItemRequest(Guid ProductId, int Quantity);
