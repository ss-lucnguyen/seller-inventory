using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;

namespace SellerInventory.Client.BlazorWeb.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly IServiceProvider _serviceProvider;
    private const string TokenKey = "authToken";

    public AuthorizationMessageHandler(ILocalStorageService localStorage, IServiceProvider serviceProvider)
    {
        _localStorage = localStorage;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>(TokenKey, cancellationToken);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Handle unauthorized responses (token expired, invalid, etc.)
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await HandleUnauthorizedAsync();
        }

        return response;
    }

    private async Task HandleUnauthorizedAsync()
    {
        // Clear the token
        await _localStorage.RemoveItemAsync(TokenKey);

        // Notify auth state provider
        try
        {
            var authStateProvider = _serviceProvider.GetService<AuthenticationStateProvider>();
            if (authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }

            // Navigate to login
            var navigationManager = _serviceProvider.GetService<NavigationManager>();
            if (navigationManager != null)
            {
                navigationManager.NavigateTo("/login", true);
            }
        }
        catch
        {
            // Ignore errors during cleanup - user will be redirected on next page load
        }
    }
}
