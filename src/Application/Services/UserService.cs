using SellerInventory.Application.DTOs.User;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserListDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapToDto(user);
    }

    public async Task<IReadOnlyList<UserListDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserListDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var existingUsers = await _unitOfWork.Users.FindAsync(
            u => u.Username == dto.Username || u.Email == dto.Email,
            cancellationToken);

        if (existingUsers.Any())
        {
            throw new InvalidOperationException("Username or email already exists");
        }

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            FullName = dto.FullName,
            Role = Enum.Parse<UserRole>(dto.Role, true),
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task<UserListDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        var existingUsers = await _unitOfWork.Users.FindAsync(
            u => u.Email == dto.Email && u.Id != id,
            cancellationToken);

        if (existingUsers.Any())
        {
            throw new InvalidOperationException("Email already exists");
        }

        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.Role = Enum.Parse<UserRole>(dto.Role, true);
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        await _unitOfWork.Users.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.IsActive;
    }

    private static UserListDto MapToDto(User user) =>
        new(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
        );
}
