namespace SellerInventory.Application.DTOs.Invoice;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    string PaymentStatus,
    decimal SubTotal,
    decimal Tax,
    decimal Discount,
    decimal Total,
    decimal AmountPaid,
    decimal AmountDue,
    string? Notes,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
