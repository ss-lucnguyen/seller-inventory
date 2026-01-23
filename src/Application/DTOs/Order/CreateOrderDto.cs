namespace SellerInventory.Application.DTOs.Order;

public record CreateOrderDto(
    IReadOnlyList<CreateOrderItemDto> Items,
    decimal Tax,
    decimal Discount,
    string? Notes
);

public record CreateOrderItemDto(Guid ProductId, int Quantity);
