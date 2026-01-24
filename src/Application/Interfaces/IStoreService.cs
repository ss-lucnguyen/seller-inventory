using SellerInventory.Application.DTOs.Store;

namespace SellerInventory.Application.Interfaces;

public interface IStoreService
{
    Task<StoreDto?> GetCurrentStoreAsync(CancellationToken cancellationToken = default);
    Task<StoreDto> UpdateStoreAsync(UpdateStoreDto dto, CancellationToken cancellationToken = default);
    Task<string?> UpdateLogoAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
}
