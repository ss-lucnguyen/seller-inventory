using SellerInventory.Application.DTOs.Auth;
using SellerInventory.Application.DTOs.Store;

namespace SellerInventory.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);

    // Multi-tenant store registration
    Task<LoginResponseDto> RegisterStoreAsync(RegisterStoreRequestDto request, CancellationToken cancellationToken = default);
    Task<InvitationDto> InviteUserAsync(Guid storeId, InviteUserRequestDto request, Guid invitedByUserId, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> AcceptInvitationAsync(AcceptInvitationRequestDto request, CancellationToken cancellationToken = default);
    Task<InvitationDto?> ValidateInvitationTokenAsync(string token, CancellationToken cancellationToken = default);
}
