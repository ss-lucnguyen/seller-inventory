using SellerInventory.Application.DTOs.Category;

namespace SellerInventory.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
