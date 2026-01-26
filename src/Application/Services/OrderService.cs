using SellerInventory.Application.DTOs.Order;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICustomerService _customerService;

    public OrderService(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICustomerService customerService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _customerService = customerService;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null) return null;

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && order.StoreId != _tenantContext.CurrentStoreId)
        {
            return null;
        }

        var user = await _unitOfWork.Users.GetByIdAsync(order.UserId, cancellationToken);
        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken);
        var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id, cancellationToken);

        return MapToDto(order, user?.Username ?? "Unknown", customer?.Name ?? "Unknown", orderItems.ToList());
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Order> orders;

        if (_tenantContext.IsSystemAdmin)
        {
            orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            orders = await _unitOfWork.Orders.FindAsync(
                o => o.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        else
        {
            return new List<OrderDto>();
        }

        var orderIds = orders.Select(o => o.Id).ToList();
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

        var users = await _unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id), cancellationToken);
        var customers = await _unitOfWork.Customers.FindAsync(c => customerIds.Contains(c.Id), cancellationToken);
        var orderItems = orderIds.Count > 0
            ? await _unitOfWork.OrderItems.FindAsync(oi => orderIds.Contains(oi.OrderId), cancellationToken)
            : new List<OrderItem>();

        var userDict = users.ToDictionary(u => u.Id, u => u.Username);
        var customerDict = customers.ToDictionary(c => c.Id, c => c.Name);
        var itemsByOrder = orderItems.GroupBy(oi => oi.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        return orders.Select(o => MapToDto(
            o,
            userDict.GetValueOrDefault(o.UserId, "Unknown"),
            customerDict.GetValueOrDefault(o.CustomerId, "Unknown"),
            itemsByOrder.GetValueOrDefault(o.Id, new List<OrderItem>())
        )).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        // Verify tenant access - user must belong to the same store
        if (!_tenantContext.IsSystemAdmin && user?.StoreId != _tenantContext.CurrentStoreId)
        {
            return new List<OrderDto>();
        }

        IReadOnlyList<Order> orders;
        if (_tenantContext.IsSystemAdmin)
        {
            orders = await _unitOfWork.Orders.FindAsync(o => o.UserId == userId, cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            orders = await _unitOfWork.Orders.FindAsync(
                o => o.UserId == userId && o.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        else
        {
            return new List<OrderDto>();
        }

        var orderIds = orders.Select(o => o.Id).ToList();
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
        var customers = await _unitOfWork.Customers.FindAsync(c => customerIds.Contains(c.Id), cancellationToken);
        var orderItems = orderIds.Count > 0
            ? await _unitOfWork.OrderItems.FindAsync(oi => orderIds.Contains(oi.OrderId), cancellationToken)
            : new List<OrderItem>();

        var customerDict = customers.ToDictionary(c => c.Id, c => c.Name);
        var itemsByOrder = orderItems.GroupBy(oi => oi.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        return orders.Select(o => MapToDto(
            o,
            user?.Username ?? "Unknown",
            customerDict.GetValueOrDefault(o.CustomerId, "Unknown"),
            itemsByOrder.GetValueOrDefault(o.Id, new List<OrderItem>())
        )).ToList();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new InvalidOperationException("Order must contain at least one item");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {userId} not found");

        if (!user.StoreId.HasValue)
        {
            throw new InvalidOperationException("User must belong to a store to create an order");
        }

        // Get or create customer
        Guid customerId;
        string customerName;
        if (dto.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId.Value, cancellationToken)
                ?? throw new KeyNotFoundException($"Customer with id {dto.CustomerId.Value} not found");

            // Verify customer belongs to the same store
            if (customer.StoreId != user.StoreId.Value)
            {
                throw new InvalidOperationException("Customer does not belong to your store");
            }

            customerId = customer.Id;
            customerName = customer.Name;
        }
        else
        {
            // Use default customer
            var defaultCustomer = await _customerService.GetOrCreateDefaultAsync(cancellationToken);
            customerId = defaultCustomer.Id;
            customerName = defaultCustomer.Name;
        }

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            SubTotal = 0,
            Tax = dto.Tax,
            Discount = dto.Discount,
            Total = 0,
            Notes = dto.Notes,
            UserId = userId,
            CustomerId = customerId,
            StoreId = user.StoreId.Value
        };

        // Add order items to the collection first
        foreach (var itemDto in dto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with id {itemDto.ProductId} not found");

            // Ensure product belongs to the same store
            if (product.StoreId != user.StoreId.Value)
            {
                throw new InvalidOperationException($"Product {product.Name} does not belong to your store");
            }

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
            }

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.SellPrice,
                Quantity = itemDto.Quantity
            };

            order.OrderItems.Add(orderItem);

            product.StockQuantity -= itemDto.Quantity;
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        }

        // Calculate totals
        order.CalculateTotal();

        // Add order (with items) to context - EF Core will handle the relationships
        await _unitOfWork.Orders.AddAsync(order, cancellationToken);

        // Save everything in one transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(order, user.Username, customerName, order.OrderItems.ToList());
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {id} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && order.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Order does not belong to your store");
        }

        order.Status = Enum.Parse<OrderStatus>(dto.Status, true);
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Users.GetByIdAsync(order.UserId, cancellationToken);
        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken);
        var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id, cancellationToken);

        return MapToDto(order, user?.Username ?? "Unknown", customer?.Name ?? "Unknown", orderItems.ToList());
    }

    public async Task<OrderDto> AddItemAsync(Guid orderId, AddOrderItemDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {orderId} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && order.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Order does not belong to your store");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with id {dto.ProductId} not found");

        // Verify product belongs to the same store
        if (product.StoreId != order.StoreId)
        {
            throw new InvalidOperationException($"Product {product.Name} does not belong to your store");
        }

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
        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken);

        return MapToDto(order, user?.Username ?? "Unknown", customer?.Name ?? "Unknown", order.OrderItems.ToList());
    }

    public async Task RemoveItemAsync(Guid orderId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {orderId} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && order.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Order does not belong to your store");
        }

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

    private static OrderDto MapToDto(Order order, string userName, string customerName, List<OrderItem> items) =>
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
            order.CustomerId,
            customerName,
            items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice,
                i.Product?.ImageUrl
            )).ToList(),
            order.CreatedAt,
            order.UpdatedAt
        );
}
