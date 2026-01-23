using SellerInventory.Domain.Enums;

namespace SellerInventory.Domain.Entities;

public class Order : BaseEntity
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

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public void CalculateTotal()
    {
        SubTotal = OrderItems.Sum(item => item.TotalPrice);
        Total = SubTotal + Tax - Discount;
    }
}
