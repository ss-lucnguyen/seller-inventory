using SellerInventory.Application.DTOs.Order;

namespace SellerInventory.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> AddItemAsync(Guid orderId, AddOrderItemDto dto, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid orderId, Guid itemId, CancellationToken cancellationToken = default);
}
