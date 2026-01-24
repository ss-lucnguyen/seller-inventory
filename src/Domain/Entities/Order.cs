using SellerInventory.Domain.Enums;
using SellerInventory.Domain.Interfaces;

namespace SellerInventory.Domain.Entities;

public class Order : BaseEntity, ITenantEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Store relationship (multi-tenant)
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public void CalculateTotal()
    {
        SubTotal = OrderItems.Sum(item => item.TotalPrice);
        Total = SubTotal + Tax - Discount;
    }
}
