using System.Net.Http.Json;
using SellerInventer.Shared.Contracts.Report;

namespace SellerInventer.Client.BlazorWeb.Services;

public class ReportService : IReportService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/reports";

    public ReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DailySalesReport?> GetDailySalesAsync(DateTime? date = null)
    {
        var url = date.HasValue
            ? $"{BaseUrl}/daily?date={date.Value:yyyy-MM-dd}"
            : $"{BaseUrl}/daily";

        return await _httpClient.GetFromJsonAsync<DailySalesReport>(url);
    }

    public async Task<SalesSummary?> GetSalesSummaryAsync(DateTime startDate, DateTime endDate)
    {
        var url = $"{BaseUrl}/summary?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await _httpClient.GetFromJsonAsync<SalesSummary>(url);
    }
}
