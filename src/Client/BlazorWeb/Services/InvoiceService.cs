using System.Net.Http.Json;
using SellerInventory.Shared.Contracts.Invoice;

namespace SellerInventory.Client.BlazorWeb.Services;

public class InvoiceService : IInvoiceService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/invoices";

    public InvoiceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<InvoiceResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<InvoiceResponse>>(BaseUrl);
        return response ?? new List<InvoiceResponse>();
    }

    public async Task<InvoiceResponse?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<InvoiceResponse>($"{BaseUrl}/{id}");
    }

    public async Task<InvoiceResponse?> GetByOrderIdAsync(Guid orderId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<InvoiceResponse>($"{BaseUrl}/by-order/{orderId}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<InvoiceResponse?> CreateAsync(CreateInvoiceRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InvoiceResponse>();
    }

    public async Task<InvoiceResponse?> UpdatePaymentAsync(Guid id, UpdateInvoicePaymentRequest request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{BaseUrl}/{id}/payment", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InvoiceResponse>();
    }

    public async Task<InvoiceResponse?> MarkAsPaidAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/mark-paid", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InvoiceResponse>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
