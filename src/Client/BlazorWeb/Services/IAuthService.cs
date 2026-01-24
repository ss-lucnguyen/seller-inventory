using SellerInventory.Shared.Contracts.Auth;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task<string?> GetTokenAsync();

    // Multi-tenant store registration
    Task<LoginResponse?> RegisterStoreAsync(RegisterStoreRequest request);
    Task<InvitationResponse?> InviteUserAsync(InviteUserRequest request);
    Task<LoginResponse?> AcceptInvitationAsync(AcceptInvitationRequest request);
    Task<InvitationResponse?> ValidateInvitationAsync(string token);
}
