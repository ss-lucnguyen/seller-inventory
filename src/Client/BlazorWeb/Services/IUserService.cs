using SellerInventory.Shared.Contracts.User;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<UserResponse?> CreateAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ResetPasswordAsync(Guid id, ResetPasswordRequest request);
    Task<bool> ToggleActiveAsync(Guid id);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
}
