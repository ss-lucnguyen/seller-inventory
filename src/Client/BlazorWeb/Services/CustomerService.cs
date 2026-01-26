using System.Net.Http.Json;
using SellerInventory.Shared.Contracts.Customer;

namespace SellerInventory.Client.BlazorWeb.Services;

public class CustomerService : ICustomerService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/customers";

    public CustomerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<CustomerResponse>>(BaseUrl);
        return response ?? new List<CustomerResponse>();
    }

    public async Task<CustomerResponse?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<CustomerResponse>($"{BaseUrl}/{id}");
    }

    public async Task<CustomerResponse?> GetDefaultAsync()
    {
        return await _httpClient.GetFromJsonAsync<CustomerResponse>($"{BaseUrl}/default");
    }

    public async Task<CustomerResponse?> CreateAsync(CreateCustomerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CustomerResponse>();
    }

    public async Task<CustomerResponse?> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CustomerResponse>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
