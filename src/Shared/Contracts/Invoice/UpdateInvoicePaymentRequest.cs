namespace SellerInventory.Shared.Contracts.Invoice;

public record UpdateInvoicePaymentRequest(
    decimal AmountPaid
);
