using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using SellerInventory.Shared.Contracts.Product;

namespace SellerInventory.Client.BlazorWeb.Services;

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

    public async Task<IReadOnlyList<ImportResultResponse>?> ImportExcelAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(10 * 1024 * 1024); // 10MB max
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        var fileContent = new ByteArrayContent(ms.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", file.Name);

        var response = await _httpClient.PostAsync($"{BaseUrl}/import-excel", content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Import failed: {error}");
        }

        return await response.Content.ReadFromJsonAsync<List<ImportResultResponse>>();
    }
}
