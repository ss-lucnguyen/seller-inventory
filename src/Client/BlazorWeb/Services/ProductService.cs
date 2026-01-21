using System.Net.Http.Json;
using SellerInventer.Shared.Contracts.Product;

namespace SellerInventer.Client.BlazorWeb.Services;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/products";

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ProductResponse>>(BaseUrl);
        return response ?? new List<ProductResponse>();
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<ProductResponse>($"{BaseUrl}/{id}");
    }

    public async Task<IReadOnlyList<ProductResponse>> GetByCategoryAsync(Guid categoryId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<ProductResponse>>($"{BaseUrl}/category/{categoryId}");
        return response ?? new List<ProductResponse>();
    }

    public async Task<ProductResponse?> CreateAsync(CreateProductRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProductResponse>();
    }

    public async Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProductResponse>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
