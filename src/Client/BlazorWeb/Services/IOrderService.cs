using SellerInventer.Shared.Contracts.Order;

namespace SellerInventer.Client.BlazorWeb.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponse>> GetAllAsync();
    Task<IReadOnlyList<OrderResponse>> GetMyOrdersAsync();
    Task<OrderResponse?> GetByIdAsync(Guid id);
    Task<OrderResponse?> CreateAsync(CreateOrderRequest request);
    Task<OrderResponse?> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request);
    Task<OrderResponse?> AddItemAsync(Guid orderId, AddOrderItemRequest request);
    Task<bool> RemoveItemAsync(Guid orderId, Guid itemId);
}
