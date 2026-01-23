using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using SellerInventory.Shared.Contracts.Auth;

namespace SellerInventory.Client.BlazorWeb.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private const string TokenKey = "authToken";

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (result is not null)
        {
            await _localStorage.SetItemAsync(TokenKey, result.Token);
            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/auth/me");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserInfo>();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }
}
