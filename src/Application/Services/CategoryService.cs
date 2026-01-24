using SellerInventory.Application.DTOs.Category;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;

namespace SellerInventory.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CategoryService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        if (category is null) return null;

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && category.StoreId != _tenantContext.CurrentStoreId)
        {
            return null;
        }

        return MapToDto(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Category> categories;

        if (_tenantContext.IsSystemAdmin)
        {
            categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            categories = await _unitOfWork.Categories.FindAsync(
                c => c.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        else
        {
            return new List<CategoryDto>();
        }

        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("No store context available");
        }

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            StoreId = _tenantContext.CurrentStoreId.Value,
            IsActive = true
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {id} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && category.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Category does not belong to your store");
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {id} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && category.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Category does not belong to your store");
        }

        await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static CategoryDto MapToDto(Category category) =>
        new(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAt,
            category.UpdatedAt
        );
}
