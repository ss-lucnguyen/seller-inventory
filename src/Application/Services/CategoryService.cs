using SellerInventory.Application.DTOs.Category;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;

namespace SellerInventory.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        return category is null ? null : MapToDto(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
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
