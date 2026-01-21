using SellerInventer.Application.DTOs.Auth;
using SellerInventer.Application.Interfaces;
using SellerInventer.Domain.Entities;
using SellerInventer.Domain.Enums;

namespace SellerInventer.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Username == request.Username, cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive");
        }

        var token = _tokenService.GenerateToken(user);

        return new LoginResponseDto(
            token,
            user.Username,
            user.FullName,
            user.Role.ToString(),
            DateTime.UtcNow.AddHours(24)
        );
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUsers = await _unitOfWork.Users.FindAsync(
            u => u.Username == request.Username || u.Email == request.Email,
            cancellationToken);

        if (existingUsers.Any())
        {
            throw new InvalidOperationException("Username or email already exists");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = Enum.Parse<UserRole>(request.Role, true),
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.IsActive
        );
    }
}
