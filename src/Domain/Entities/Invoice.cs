using SellerInventory.Domain.Enums;
using SellerInventory.Domain.Interfaces;

namespace SellerInventory.Domain.Entities;

public class Invoice : BaseEntity, ITenantEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.NotPaid;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue => Total - AmountPaid;
    public string? Notes { get; set; }

    // Order relationship (1:1)
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Store relationship (multi-tenant)
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public void UpdatePaymentStatus()
    {
        if (AmountPaid >= Total)
        {
            PaymentStatus = InvoicePaymentStatus.Paid;
        }
        else if (AmountPaid > 0)
        {
            PaymentStatus = InvoicePaymentStatus.PartialPaid;
        }
        else
        {
            PaymentStatus = InvoicePaymentStatus.NotPaid;
        }
    }
}
