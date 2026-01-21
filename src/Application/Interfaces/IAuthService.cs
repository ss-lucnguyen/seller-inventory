using SellerInventer.Application.DTOs.Auth;

namespace SellerInventer.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
}
