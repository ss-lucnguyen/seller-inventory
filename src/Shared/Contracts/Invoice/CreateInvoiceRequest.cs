namespace SellerInventory.Shared.Contracts.Invoice;

public record CreateInvoiceRequest(
    Guid OrderId,
    DateTime? DueDate,
    string? Notes
);
