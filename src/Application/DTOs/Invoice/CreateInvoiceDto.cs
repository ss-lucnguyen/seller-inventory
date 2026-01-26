namespace SellerInventory.Application.DTOs.Invoice;

public record CreateInvoiceDto(
    Guid OrderId,
    DateTime? DueDate,
    string? Notes
);
