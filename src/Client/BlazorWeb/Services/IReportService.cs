using SellerInventer.Shared.Contracts.Report;

namespace SellerInventer.Client.BlazorWeb.Services;

public interface IReportService
{
    Task<DailySalesReport?> GetDailySalesAsync(DateTime? date = null);
    Task<SalesSummary?> GetSalesSummaryAsync(DateTime startDate, DateTime endDate);
}
