using System.Net.Http.Json;
using SellerInventory.Shared.Contracts.User;

namespace SellerInventory.Client.BlazorWeb.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/users";

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<UserResponse>>(BaseUrl);
        return response ?? new List<UserResponse>();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<UserResponse>($"{BaseUrl}/{id}");
    }

    public async Task<UserResponse?> CreateAsync(CreateUserRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserResponse>();
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserResponse>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(Guid id, ResetPasswordRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/{id}/reset-password", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/toggle-active", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/change-password", request);
        return response.IsSuccessStatusCode;
    }
}
