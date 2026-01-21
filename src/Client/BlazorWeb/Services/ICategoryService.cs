using SellerInventer.Shared.Contracts.Category;

namespace SellerInventer.Client.BlazorWeb.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse?> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(Guid id);
}
