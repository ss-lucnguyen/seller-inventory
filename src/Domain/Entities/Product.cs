namespace SellerInventory.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
