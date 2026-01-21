namespace SellerInventer.Application.DTOs.Order;

public record OrderDto(
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
    IReadOnlyList<OrderItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
