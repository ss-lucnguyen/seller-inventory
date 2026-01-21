namespace SellerInventer.Shared.Contracts.Report;

public record DailySalesReport(
    DateTime Date,
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    IReadOnlyList<TopSellingProduct> TopSellingProducts
);

public record TopSellingProduct(
    Guid ProductId,
    string ProductName,
    int QuantitySold,
    decimal Revenue
);
