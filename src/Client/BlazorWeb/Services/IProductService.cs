using SellerInventory.Shared.Contracts.Product;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync();
    Task<ProductResponse?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<ProductResponse>> GetByCategoryAsync(Guid categoryId);
    Task<ProductResponse?> CreateAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<IReadOnlyList<ImportResultResponse>?> ImportExcelAsync(byte[] fileData, string fileName);
}
