namespace SellerInventer.Shared.Contracts.Report;

public record SalesSummary(
    DateTime StartDate,
    DateTime EndDate,
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    int TotalProductsSold
);
