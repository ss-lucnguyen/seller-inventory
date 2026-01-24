namespace SellerInventory.Client.BlazorWeb.Services;

public interface IStoreContext
{
    Guid? CurrentStoreId { get; }
    string? CurrentStoreName { get; }
    bool HasStoreContext { get; }

    event Action? OnStoreContextChanged;

    Task InitializeAsync();
    Task SetStoreContextAsync(Guid? storeId, string? storeName);
    Task ClearStoreContextAsync();
}
