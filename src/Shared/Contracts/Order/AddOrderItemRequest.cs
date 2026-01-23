namespace SellerInventory.Shared.Contracts.Order;

public record AddOrderItemRequest(Guid ProductId, int Quantity);
