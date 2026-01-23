using SellerInventory.Application.DTOs.Product;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;

namespace SellerInventory.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null) return null;

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, cancellationToken);
        return MapToDto(product, category?.Name ?? "Unknown");
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return products.Select(p => MapToDto(p, categoryDict.GetValueOrDefault(p.CategoryId, "Unknown"))).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.FindAsync(p => p.CategoryId == categoryId, cancellationToken);
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
        var categoryName = category?.Name ?? "Unknown";

        return products.Select(p => MapToDto(p, categoryName)).ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found");

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            CostPrice = dto.CostPrice,
            SellPrice = dto.SellPrice,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product, category.Name);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with id {id} not found");

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.SKU = dto.SKU;
        product.CostPrice = dto.CostPrice;
        product.SellPrice = dto.SellPrice;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product, category.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with id {id} not found");

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStockAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with id {id} not found");

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ImportResultDto>> ImportAsync(IEnumerable<ImportProductDto> products, CancellationToken cancellationToken = default)
    {
        var results = new List<ImportResultDto>();
        var productList = products.ToList();

        try
        {
            // Get or create categories
            var categoryNames = productList.Select(p => p.CategoryName).Distinct().ToList();
            var existingCategories = await _unitOfWork.Categories.FindAsync(
                c => categoryNames.Contains(c.Name), cancellationToken);

            var existingCategoryDict = existingCategories.ToDictionary(c => c.Name, c => c.Id);
            var newCategories = new List<Category>();

            foreach (var categoryName in categoryNames.Where(cn => !existingCategoryDict.ContainsKey(cn)))
            {
                var category = new Category { Name = categoryName };
                newCategories.Add(category);
                existingCategoryDict[categoryName] = category.Id;
            }

            if (newCategories.Any())
            {
                foreach (var category in newCategories)
                {
                    await _unitOfWork.Categories.AddAsync(category, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Create products
            foreach (var dto in productList)
            {
                try
                {
                    var product = new Product
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        SKU = dto.SKU,
                        CostPrice = dto.CostPrice,
                        SellPrice = dto.SellPrice,
                        StockQuantity = dto.StockQuantity,
                        CategoryId = existingCategoryDict[dto.CategoryName],
                        IsActive = true
                    };

                    await _unitOfWork.Products.AddAsync(product, cancellationToken);
                    results.Add(new ImportResultDto(dto.Name, true, "Success"));
                }
                catch (Exception ex)
                {
                    results.Add(new ImportResultDto(dto.Name, false, ex.Message));
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            foreach (var product in productList.Where(p => !results.Any(r => r.ProductName == p.Name)))
            {
                results.Add(new ImportResultDto(product.Name, false, ex.Message));
            }
        }

        return results.AsReadOnly();
    }

    private static ProductDto MapToDto(Product product, string categoryName) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.SKU,
            product.CostPrice,
            product.SellPrice,
            product.StockQuantity,
            product.IsActive,
            product.CategoryId,
            categoryName,
            product.ImageUrl,
            product.CreatedAt,
            product.UpdatedAt
        );
}
