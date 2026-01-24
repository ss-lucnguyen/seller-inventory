using SellerInventory.Application.DTOs.Report;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ReportService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<DailySalesReportDto> GetDailySalesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasStoreAccess)
        {
            throw new InvalidOperationException("User must belong to a store to view reports");
        }

        // Ensure date is UTC
        var utcDate = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        var startOfDay = utcDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        var storeId = _tenantContext.CurrentStoreId ?? throw new InvalidOperationException("User must belong to a store to view reports");

        var orders = (await _unitOfWork.Orders.FindAsync(
            o => o.OrderDate >= startOfDay && o.OrderDate < endOfDay && o.Status == OrderStatus.Completed && o.StoreId == storeId,
            cancellationToken)).ToList();

        var orderIds = orders.Select(o => o.Id).ToList();

        // Only get order items for the orders we found
        var orderItems = orderIds.Count > 0
            ? (await _unitOfWork.OrderItems.FindAsync(oi => orderIds.Contains(oi.OrderId), cancellationToken)).ToList()
            : new List<Domain.Entities.OrderItem>();

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
        if (!_tenantContext.HasStoreAccess)
        {
            throw new InvalidOperationException("User must belong to a store to view reports");
        }

        // Ensure dates are UTC
        var utcStartDate = startDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc)
            : startDate.ToUniversalTime();

        var utcEndDate = endDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc)
            : endDate.ToUniversalTime();

        var storeId = _tenantContext.CurrentStoreId ?? throw new InvalidOperationException("User must belong to a store to view reports");

        var orders = (await _unitOfWork.Orders.FindAsync(
            o => o.OrderDate >= utcStartDate && o.OrderDate <= utcEndDate && o.Status == OrderStatus.Completed && o.StoreId == storeId,
            cancellationToken)).ToList();

        var orderIds = orders.Select(o => o.Id).ToList();

        // Only get order items for the orders we found
        var orderItems = orderIds.Count > 0
            ? (await _unitOfWork.OrderItems.FindAsync(oi => orderIds.Contains(oi.OrderId), cancellationToken)).ToList()
            : new List<Domain.Entities.OrderItem>();

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.Total);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
        var totalProductsSold = orderItems.Sum(oi => oi.Quantity);

        return new SalesSummaryDto(
            utcStartDate,
            utcEndDate,
            totalOrders,
            totalRevenue,
            averageOrderValue,
            totalProductsSold
        );
    }
}
