using SellerInventory.Shared.Contracts.Invoice;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceResponse>> GetAllAsync();
    Task<InvoiceResponse?> GetByIdAsync(Guid id);
    Task<InvoiceResponse?> GetByOrderIdAsync(Guid orderId);
    Task<InvoiceResponse?> CreateAsync(CreateInvoiceRequest request);
    Task<InvoiceResponse?> UpdatePaymentAsync(Guid id, UpdateInvoicePaymentRequest request);
    Task<InvoiceResponse?> MarkAsPaidAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
