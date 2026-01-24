using SellerInventory.Shared.Contracts.Store;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface IStoreSettingsService
{
    Task<StoreResponse?> GetCurrentStoreAsync();
    Task<StoreResponse?> UpdateStoreAsync(UpdateStoreRequest request);
    Task<string?> UploadLogoAsync(Stream fileStream, string fileName);
}
