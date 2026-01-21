using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace SellerInventer.Client.BlazorWeb.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private const string TokenKey = "authToken";

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(TokenKey);

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        if (expiry is not null && long.TryParse(expiry, out var expiryTime))
        {
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryTime).UtcDateTime;
            if (expiryDateTime < DateTime.UtcNow)
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}
