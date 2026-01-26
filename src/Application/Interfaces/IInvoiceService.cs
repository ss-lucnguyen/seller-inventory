using SellerInventory.Application.DTOs.Invoice;

namespace SellerInventory.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<InvoiceDto> UpdatePaymentAsync(Guid id, UpdateInvoicePaymentDto dto, CancellationToken cancellationToken = default);
    Task<InvoiceDto> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
