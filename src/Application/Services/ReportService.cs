using SellerInventer.Application.DTOs.Report;
using SellerInventer.Application.Interfaces;
using SellerInventer.Domain.Enums;

namespace SellerInventer.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DailySalesReportDto> GetDailySalesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var orders = await _unitOfWork.Orders.FindAsync(
            o => o.OrderDate >= startOfDay && o.OrderDate < endOfDay && o.Status == OrderStatus.Completed,
            cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();
        var allOrderItems = await _unitOfWork.OrderItems.GetAllAsync(cancellationToken);
        var orderItems = allOrderItems.Where(oi => orderIds.Contains(oi.OrderId)).ToList();

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.Total);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var topSellingProducts = orderItems
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new TopSellingProductDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.TotalPrice)
            ))
            .OrderByDescending(p => p.QuantitySold)
            .Take(10)
            .ToList();

        return new DailySalesReportDto(
            date,
            totalOrders,
            totalRevenue,
            averageOrderValue,
            topSellingProducts
        );
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.FindAsync(
            o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed,
            cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();
        var allOrderItems = await _unitOfWork.OrderItems.GetAllAsync(cancellationToken);
        var orderItems = allOrderItems.Where(oi => orderIds.Contains(oi.OrderId)).ToList();

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.Total);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
        var totalProductsSold = orderItems.Sum(oi => oi.Quantity);

        return new SalesSummaryDto(
            startDate,
            endDate,
            totalOrders,
            totalRevenue,
            averageOrderValue,
            totalProductsSold
        );
    }
}
