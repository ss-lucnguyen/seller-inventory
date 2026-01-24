using SellerInventory.Application.DTOs.Auth;
using SellerInventory.Application.DTOs.Store;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Services;

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

        // Get store info if user belongs to a store
        string? storeName = null;
        if (user.StoreId.HasValue)
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(user.StoreId.Value, cancellationToken);
            storeName = store?.Name;

            // Check if store is active
            if (store != null && !store.IsActive)
            {
                throw new UnauthorizedAccessException("Store is inactive");
            }
        }

        var token = _tokenService.GenerateToken(user);

        return new LoginResponseDto(
            token,
            user.Username,
            user.FullName,
            user.Role.ToString(),
            user.StoreId,
            storeName,
            DateTime.UtcNow.AddHours(24)
        );
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // This method is now only for SystemAdmin to create users
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

        // Get store name if user belongs to a store
        string? storeName = null;
        if (user.StoreId.HasValue)
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(user.StoreId.Value, cancellationToken);
            storeName = store?.Name;
        }

        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.IsActive,
            user.StoreId,
            storeName
        );
    }

    public async Task<LoginResponseDto> RegisterStoreAsync(RegisterStoreRequestDto request, CancellationToken cancellationToken = default)
    {
        // Check if store slug already exists
        var existingStores = await _unitOfWork.Stores.FindAsync(s => s.Slug == request.StoreSlug, cancellationToken);
        if (existingStores.Any())
        {
            throw new InvalidOperationException("Store slug already exists");
        }

        // Check if username or email already exists
        var existingUsers = await _unitOfWork.Users.FindAsync(
            u => u.Username == request.OwnerUsername || u.Email == request.OwnerEmail,
            cancellationToken);
        if (existingUsers.Any())
        {
            throw new InvalidOperationException("Username or email already exists");
        }

        // Create the store
        var store = new Store
        {
            Name = request.StoreName,
            Slug = request.StoreSlug.ToLower().Replace(" ", "-"),
            Location = request.Location,
            Address = request.Address,
            Industry = request.Industry,
            Currency = request.Currency ?? "USD",
            IsActive = true,
            SubscriptionStatus = SubscriptionStatus.Trial,
            SubscriptionExpiresAt = DateTime.UtcNow.AddDays(14) // 14-day trial
        };

        await _unitOfWork.Stores.AddAsync(store, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create the owner user as Manager
        var owner = new User
        {
            Username = request.OwnerUsername,
            Email = request.OwnerEmail,
            PasswordHash = _passwordHasher.Hash(request.OwnerPassword),
            FullName = request.OwnerFullName,
            Role = UserRole.Manager, // Store owner is a Manager
            StoreId = store.Id,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(owner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate token for auto-login
        var token = _tokenService.GenerateToken(owner);

        return new LoginResponseDto(
            token,
            owner.Username,
            owner.FullName,
            owner.Role.ToString(),
            store.Id,
            store.Name,
            DateTime.UtcNow.AddHours(24)
        );
    }

    public async Task<InvitationDto> InviteUserAsync(Guid storeId, InviteUserRequestDto request, Guid invitedByUserId, CancellationToken cancellationToken = default)
    {
        // Verify store exists
        var store = await _unitOfWork.Stores.GetByIdAsync(storeId, cancellationToken)
            ?? throw new KeyNotFoundException("Store not found");

        // Check if email already has pending invitation for this store
        var existingInvitations = await _unitOfWork.StoreInvitations.FindAsync(
            i => i.Email == request.Email && i.StoreId == storeId && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow,
            cancellationToken);
        if (existingInvitations.Any())
        {
            throw new InvalidOperationException("An invitation has already been sent to this email");
        }

        // Check if user with this email already exists in this store
        var existingUsers = await _unitOfWork.Users.FindAsync(
            u => u.Email == request.Email && u.StoreId == storeId,
            cancellationToken);
        if (existingUsers.Any())
        {
            throw new InvalidOperationException("A user with this email already exists in this store");
        }

        // Parse role (only Staff or Manager allowed)
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role) || role == UserRole.SystemAdmin)
        {
            throw new InvalidOperationException("Invalid role. Only Staff or Manager allowed");
        }

        // Create invitation
        var invitation = new StoreInvitation
        {
            StoreId = storeId,
            Email = request.Email,
            Role = role,
            Token = GenerateInvitationToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7-day expiration
            InvitedByUserId = invitedByUserId,
            IsUsed = false
        };

        await _unitOfWork.StoreInvitations.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            store.Name,
            invitation.ExpiresAt,
            invitation.IsUsed
        );
    }

    public async Task<LoginResponseDto> AcceptInvitationAsync(AcceptInvitationRequestDto request, CancellationToken cancellationToken = default)
    {
        // Find the invitation
        var invitations = await _unitOfWork.StoreInvitations.FindAsync(
            i => i.Token == request.Token && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow,
            cancellationToken);
        var invitation = invitations.FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid or expired invitation token");

        // Check if username already exists
        var existingUsers = await _unitOfWork.Users.FindAsync(
            u => u.Username == request.Username,
            cancellationToken);
        if (existingUsers.Any())
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Get store
        var store = await _unitOfWork.Stores.GetByIdAsync(invitation.StoreId, cancellationToken)
            ?? throw new KeyNotFoundException("Store not found");

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = invitation.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = invitation.Role,
            StoreId = invitation.StoreId,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        // Mark invitation as used
        invitation.IsUsed = true;
        await _unitOfWork.StoreInvitations.UpdateAsync(invitation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _tokenService.GenerateToken(user);

        return new LoginResponseDto(
            token,
            user.Username,
            user.FullName,
            user.Role.ToString(),
            store.Id,
            store.Name,
            DateTime.UtcNow.AddHours(24)
        );
    }

    public async Task<InvitationDto?> ValidateInvitationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var invitations = await _unitOfWork.StoreInvitations.FindAsync(
            i => i.Token == token && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow,
            cancellationToken);
        var invitation = invitations.FirstOrDefault();

        if (invitation == null)
        {
            return null;
        }

        var store = await _unitOfWork.Stores.GetByIdAsync(invitation.StoreId, cancellationToken);

        return new InvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            store?.Name ?? "Unknown Store",
            invitation.ExpiresAt,
            invitation.IsUsed
        );
    }

    private static string GenerateInvitationToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }
}
