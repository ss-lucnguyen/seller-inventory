namespace SellerInventer.Application.DTOs.Report;

public record DailySalesReportDto(
    DateTime Date,
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    IReadOnlyList<TopSellingProductDto> TopSellingProducts
);

public record TopSellingProductDto(
    Guid ProductId,
    string ProductName,
    int QuantitySold,
    decimal Revenue
);
