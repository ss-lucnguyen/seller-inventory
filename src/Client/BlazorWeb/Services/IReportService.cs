using SellerInventory.Shared.Contracts.Report;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface IReportService
{
    Task<DailySalesReport?> GetDailySalesAsync(DateTime? date = null);
    Task<SalesSummary?> GetSalesSummaryAsync(DateTime startDate, DateTime endDate);
}
