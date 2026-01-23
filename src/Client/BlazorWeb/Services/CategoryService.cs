using System.Net.Http.Json;
using SellerInventory.Shared.Contracts.Category;

namespace SellerInventory.Client.BlazorWeb.Services;

public class CategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/categories";

    public CategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<CategoryResponse>>(BaseUrl);
        return response ?? new List<CategoryResponse>();
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<CategoryResponse>($"{BaseUrl}/{id}");
    }

    public async Task<CategoryResponse?> CreateAsync(CreateCategoryRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CategoryResponse>();
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CategoryResponse>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
