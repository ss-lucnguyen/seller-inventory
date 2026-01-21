using SellerInventer.Shared.Contracts.Auth;

namespace SellerInventer.Client.BlazorWeb.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task<string?> GetTokenAsync();
}
