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
    private const string StoreIdKey = "storeId";
    private const string StoreNameKey = "storeName";

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
            if (result.StoreId.HasValue)
            {
                await _localStorage.SetItemAsync(StoreIdKey, result.StoreId.Value.ToString());
                await _localStorage.SetItemAsync(StoreNameKey, result.StoreName ?? "");
            }
            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(StoreIdKey);
        await _localStorage.RemoveItemAsync(StoreNameKey);
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

    public async Task<LoginResponse?> RegisterStoreAsync(RegisterStoreRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register-store", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (result is not null)
        {
            await _localStorage.SetItemAsync(TokenKey, result.Token);
            if (result.StoreId.HasValue)
            {
                await _localStorage.SetItemAsync(StoreIdKey, result.StoreId.Value.ToString());
                await _localStorage.SetItemAsync(StoreNameKey, result.StoreName ?? "");
            }
            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
        }

        return result;
    }

    public async Task<InvitationResponse?> InviteUserAsync(InviteUserRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/invite", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<InvitationResponse>();
    }

    public async Task<LoginResponse?> AcceptInvitationAsync(AcceptInvitationRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/accept-invitation", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (result is not null)
        {
            await _localStorage.SetItemAsync(TokenKey, result.Token);
            if (result.StoreId.HasValue)
            {
                await _localStorage.SetItemAsync(StoreIdKey, result.StoreId.Value.ToString());
                await _localStorage.SetItemAsync(StoreNameKey, result.StoreName ?? "");
            }
            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
        }

        return result;
    }

    public async Task<InvitationResponse?> ValidateInvitationAsync(string token)
    {
        var response = await _httpClient.GetAsync($"api/v1/auth/validate-invitation/{token}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<InvitationResponse>();
    }
}
