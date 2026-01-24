using SellerInventory.Application.DTOs.Product;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;

namespace SellerInventory.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ProductService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null) return null;

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && product.StoreId != _tenantContext.CurrentStoreId)
        {
            return null;
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, cancellationToken);
        return MapToDto(product, category?.Name ?? "Unknown");
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> products;

        if (_tenantContext.IsSystemAdmin)
        {
            products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            products = await _unitOfWork.Products.FindAsync(
                p => p.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        else
        {
            return new List<ProductDto>();
        }

        var categories = await GetCategoriesForTenantAsync(cancellationToken);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return products.Select(p => MapToDto(p, categoryDict.GetValueOrDefault(p.CategoryId, "Unknown"))).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> products;

        if (_tenantContext.IsSystemAdmin)
        {
            products = await _unitOfWork.Products.FindAsync(p => p.CategoryId == categoryId, cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            products = await _unitOfWork.Products.FindAsync(
                p => p.CategoryId == categoryId && p.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        else
        {
            return new List<ProductDto>();
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
        var categoryName = category?.Name ?? "Unknown";

        return products.Select(p => MapToDto(p, categoryName)).ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("No store context available");
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found");

        // Verify category belongs to the same store
        if (!_tenantContext.IsSystemAdmin && category.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Category does not belong to your store");
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            CostPrice = dto.CostPrice,
            SellPrice = dto.SellPrice,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            ImageUrl = dto.ImageUrl,
            StoreId = _tenantContext.CurrentStoreId.Value,
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

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && product.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Product does not belong to your store");
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found");

        // Verify category belongs to the same store
        if (!_tenantContext.IsSystemAdmin && category.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Category does not belong to your store");
        }

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.SKU = dto.SKU;
        product.CostPrice = dto.CostPrice;
        product.SellPrice = dto.SellPrice;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.ImageUrl = dto.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product, category.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with id {id} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && product.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Product does not belong to your store");
        }

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStockAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with id {id} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && product.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Product does not belong to your store");
        }

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ImportResultDto>> ImportAsync(IEnumerable<ImportProductDto> products, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("No store context available");
        }

        var results = new List<ImportResultDto>();
        var productList = products.ToList();
        var storeId = _tenantContext.CurrentStoreId.Value;

        try
        {
            // Get or create categories for this store
            var categoryNames = productList.Select(p => p.CategoryName).Distinct().ToList();
            var existingCategories = await _unitOfWork.Categories.FindAsync(
                c => categoryNames.Contains(c.Name) && c.StoreId == storeId, cancellationToken);

            var existingCategoryDict = existingCategories.ToDictionary(c => c.Name, c => c.Id);
            var newCategories = new List<Category>();

            foreach (var categoryName in categoryNames.Where(cn => !existingCategoryDict.ContainsKey(cn)))
            {
                var category = new Category { Name = categoryName, StoreId = storeId };
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
                        StoreId = storeId,
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

    private async Task<IReadOnlyList<Category>> GetCategoriesForTenantAsync(CancellationToken cancellationToken)
    {
        if (_tenantContext.IsSystemAdmin)
        {
            return await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            return await _unitOfWork.Categories.FindAsync(
                c => c.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        return new List<Category>();
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
