using SellerInventer.Application.DTOs.User;

namespace SellerInventer.Application.Interfaces;

public interface IUserService
{
    Task<UserListDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserListDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserListDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserListDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(Guid id, ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<bool> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);
}
