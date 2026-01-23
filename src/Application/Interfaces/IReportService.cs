using SellerInventory.Application.DTOs.Report;

namespace SellerInventory.Application.Interfaces;

public interface IReportService
{
    Task<DailySalesReportDto> GetDailySalesAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
