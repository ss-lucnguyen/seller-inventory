using SellerInventory.Shared.Contracts.Category;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse?> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(Guid id);
}
