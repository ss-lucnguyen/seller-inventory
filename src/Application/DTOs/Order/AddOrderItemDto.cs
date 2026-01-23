namespace SellerInventory.Application.DTOs.Order;

public record AddOrderItemDto(Guid ProductId, int Quantity);
