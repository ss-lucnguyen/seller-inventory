using SellerInventory.Application.DTOs.Product;

namespace SellerInventory.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateStockAsync(Guid id, int quantity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportResultDto>> ImportAsync(IEnumerable<ImportProductDto> products, CancellationToken cancellationToken = default);
}
