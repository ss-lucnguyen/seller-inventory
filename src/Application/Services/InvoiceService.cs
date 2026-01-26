using SellerInventory.Application.DTOs.Invoice;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public InvoiceService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(id, cancellationToken);
        if (invoice is null) return null;

        if (!_tenantContext.IsSystemAdmin && invoice.StoreId != _tenantContext.CurrentStoreId)
        {
            return null;
        }

        var order = await _unitOfWork.Orders.GetByIdAsync(invoice.OrderId, cancellationToken);
        var customer = order != null
            ? await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken)
            : null;
        return MapToDto(invoice, order?.OrderNumber ?? "Unknown", order?.CustomerId ?? Guid.Empty, customer?.Name ?? "Unknown");
    }

    public async Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var invoices = await _unitOfWork.Invoices.FindAsync(i => i.OrderId == orderId, cancellationToken);
        var invoice = invoices.FirstOrDefault();
        if (invoice is null) return null;

        if (!_tenantContext.IsSystemAdmin && invoice.StoreId != _tenantContext.CurrentStoreId)
        {
            return null;
        }

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        var customer = order != null
            ? await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken)
            : null;
        return MapToDto(invoice, order?.OrderNumber ?? "Unknown", order?.CustomerId ?? Guid.Empty, customer?.Name ?? "Unknown");
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Invoice> invoices;

        if (_tenantContext.IsSystemAdmin)
        {
            invoices = await _unitOfWork.Invoices.GetAllAsync(cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            invoices = await _unitOfWork.Invoices.FindAsync(
                i => i.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        else
        {
            return new List<InvoiceDto>();
        }

        var orderIds = invoices.Select(i => i.OrderId).Distinct().ToList();
        var orders = orderIds.Count > 0
            ? await _unitOfWork.Orders.FindAsync(o => orderIds.Contains(o.Id), cancellationToken)
            : new List<Order>();

        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
        var customers = customerIds.Count > 0
            ? await _unitOfWork.Customers.FindAsync(c => customerIds.Contains(c.Id), cancellationToken)
            : new List<Customer>();

        var orderDict = orders.ToDictionary(o => o.Id, o => (OrderNumber: o.OrderNumber, CustomerId: o.CustomerId));
        var customerDict = customers.ToDictionary(c => c.Id, c => c.Name);

        return invoices.Select(i =>
        {
            var orderInfo = orderDict.GetValueOrDefault(i.OrderId, (OrderNumber: "Unknown", CustomerId: Guid.Empty));
            var customerName = customerDict.GetValueOrDefault(orderInfo.CustomerId, "Unknown");
            return MapToDto(i, orderInfo.OrderNumber, orderInfo.CustomerId, customerName);
        }).ToList();
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {dto.OrderId} not found");

        if (!_tenantContext.IsSystemAdmin && order.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Order does not belong to your store");
        }

        if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException("Invoice can only be created for confirmed or completed orders");
        }

        var existingInvoices = await _unitOfWork.Invoices.FindAsync(i => i.OrderId == dto.OrderId, cancellationToken);
        if (existingInvoices.Any())
        {
            throw new InvalidOperationException("An invoice already exists for this order");
        }

        var invoice = new Invoice
        {
            InvoiceNumber = GenerateInvoiceNumber(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = dto.DueDate.HasValue
                ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                : null,
            PaymentStatus = InvoicePaymentStatus.NotPaid,
            SubTotal = order.SubTotal,
            Tax = order.Tax,
            Discount = order.Discount,
            Total = order.Total,
            AmountPaid = 0,
            Notes = dto.Notes,
            OrderId = order.Id,
            StoreId = order.StoreId
        };

        await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken);
        return MapToDto(invoice, order.OrderNumber, order.CustomerId, customer?.Name ?? "Unknown");
    }

    public async Task<InvoiceDto> UpdatePaymentAsync(Guid id, UpdateInvoicePaymentDto dto, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice with id {id} not found");

        if (!_tenantContext.IsSystemAdmin && invoice.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Invoice does not belong to your store");
        }

        if (dto.AmountPaid < 0)
        {
            throw new InvalidOperationException("Amount paid cannot be negative");
        }

        if (dto.AmountPaid > invoice.Total)
        {
            throw new InvalidOperationException("Amount paid cannot exceed the total amount");
        }

        invoice.AmountPaid = dto.AmountPaid;
        invoice.UpdatePaymentStatus();
        invoice.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Invoices.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var order = await _unitOfWork.Orders.GetByIdAsync(invoice.OrderId, cancellationToken);
        var customer = order != null
            ? await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken)
            : null;
        return MapToDto(invoice, order?.OrderNumber ?? "Unknown", order?.CustomerId ?? Guid.Empty, customer?.Name ?? "Unknown");
    }

    public async Task<InvoiceDto> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice with id {id} not found");

        if (!_tenantContext.IsSystemAdmin && invoice.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Invoice does not belong to your store");
        }

        invoice.AmountPaid = invoice.Total;
        invoice.PaymentStatus = InvoicePaymentStatus.Paid;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Invoices.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var order = await _unitOfWork.Orders.GetByIdAsync(invoice.OrderId, cancellationToken);
        var customer = order != null
            ? await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken)
            : null;
        return MapToDto(invoice, order?.OrderNumber ?? "Unknown", order?.CustomerId ?? Guid.Empty, customer?.Name ?? "Unknown");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice with id {id} not found");

        if (!_tenantContext.IsSystemAdmin && invoice.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Invoice does not belong to your store");
        }

        if (invoice.AmountPaid > 0)
        {
            throw new InvalidOperationException("Cannot delete an invoice that has received payments");
        }

        await _unitOfWork.Invoices.DeleteAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateInvoiceNumber()
    {
        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static InvoiceDto MapToDto(Invoice invoice, string orderNumber, Guid customerId, string customerName) =>
        new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.PaymentStatus.ToString(),
            invoice.SubTotal,
            invoice.Tax,
            invoice.Discount,
            invoice.Total,
            invoice.AmountPaid,
            invoice.AmountDue,
            invoice.Notes,
            invoice.OrderId,
            orderNumber,
            customerId,
            customerName,
            invoice.CreatedAt,
            invoice.UpdatedAt
        );
}
