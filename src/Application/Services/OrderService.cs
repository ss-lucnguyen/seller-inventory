using SellerInventory.Application.DTOs.Order;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null) return null;

        var user = await _unitOfWork.Users.GetByIdAsync(order.UserId, cancellationToken);
        var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id, cancellationToken);

        return MapToDto(order, user?.Username ?? "Unknown", orderItems.ToList());
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        var orderItems = await _unitOfWork.OrderItems.GetAllAsync(cancellationToken);

        var userDict = users.ToDictionary(u => u.Id, u => u.Username);
        var itemsByOrder = orderItems.GroupBy(oi => oi.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        return orders.Select(o => MapToDto(
            o,
            userDict.GetValueOrDefault(o.UserId, "Unknown"),
            itemsByOrder.GetValueOrDefault(o.Id, new List<OrderItem>())
        )).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.FindAsync(o => o.UserId == userId, cancellationToken);
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        var orderItems = await _unitOfWork.OrderItems.GetAllAsync(cancellationToken);

        var itemsByOrder = orderItems.GroupBy(oi => oi.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        return orders.Select(o => MapToDto(
            o,
            user?.Username ?? "Unknown",
            itemsByOrder.GetValueOrDefault(o.Id, new List<OrderItem>())
        )).ToList();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {userId} not found");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Tax = dto.Tax,
            Discount = dto.Discount,
            Notes = dto.Notes,
            UserId = userId
        };

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);

        foreach (var itemDto in dto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with id {itemDto.ProductId} not found");

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
            }

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.SellPrice,
                Quantity = itemDto.Quantity
            };

            await _unitOfWork.OrderItems.AddAsync(orderItem, cancellationToken);
            order.OrderItems.Add(orderItem);

            product.StockQuantity -= itemDto.Quantity;
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        }

        order.CalculateTotal();
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(order, user.Username, order.OrderItems.ToList());
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {id} not found");

        order.Status = Enum.Parse<OrderStatus>(dto.Status, true);
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Users.GetByIdAsync(order.UserId, cancellationToken);
        var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id, cancellationToken);

        return MapToDto(order, user?.Username ?? "Unknown", orderItems.ToList());
    }

    public async Task<OrderDto> AddItemAsync(Guid orderId, AddOrderItemDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {orderId} not found");

        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with id {dto.ProductId} not found");

        if (product.StockQuantity < dto.Quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
        }

        var orderItem = new OrderItem
        {
            OrderId = orderId,
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.SellPrice,
            Quantity = dto.Quantity
        };

        await _unitOfWork.OrderItems.AddAsync(orderItem, cancellationToken);

        product.StockQuantity -= dto.Quantity;
        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);

        var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == orderId, cancellationToken);
        order.OrderItems = orderItems.ToList();
        order.CalculateTotal();
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Users.GetByIdAsync(order.UserId, cancellationToken);

        return MapToDto(order, user?.Username ?? "Unknown", order.OrderItems.ToList());
    }

    public async Task RemoveItemAsync(Guid orderId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {orderId} not found");

        var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(itemId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order item with id {itemId} not found");

        var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId, cancellationToken);
        if (product is not null)
        {
            product.StockQuantity += orderItem.Quantity;
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        }

        await _unitOfWork.OrderItems.DeleteAsync(orderItem, cancellationToken);

        var remainingItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == orderId, cancellationToken);
        order.OrderItems = remainingItems.ToList();
        order.CalculateTotal();
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static OrderDto MapToDto(Order order, string userName, List<OrderItem> items) =>
        new(
            order.Id,
            order.OrderNumber,
            order.OrderDate,
            order.Status.ToString(),
            order.SubTotal,
            order.Tax,
            order.Discount,
            order.Total,
            order.Notes,
            order.UserId,
            userName,
            items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice
            )).ToList(),
            order.CreatedAt,
            order.UpdatedAt
        );
}
