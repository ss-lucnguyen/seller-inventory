namespace SellerInventory.Application.DTOs.Report;

public record SalesSummaryDto(
    DateTime StartDate,
    DateTime EndDate,
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    int TotalProductsSold
);
