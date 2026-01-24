using System.Net.Http.Json;
using SellerInventory.Shared.Contracts.Store;

namespace SellerInventory.Client.BlazorWeb.Services;

public class StoreSettingsService : IStoreSettingsService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/store";

    public StoreSettingsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StoreResponse?> GetCurrentStoreAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<StoreResponse>(BaseUrl);
        }
        catch
        {
            return null;
        }
    }

    public async Task<StoreResponse?> UpdateStoreAsync(UpdateStoreRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<StoreResponse>();
    }

    public async Task<string?> UploadLogoAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync($"{BaseUrl}/logo", content);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<LogoUploadResponse>();
        return result?.LogoUrl;
    }

    private record LogoUploadResponse(string LogoUrl);
}
