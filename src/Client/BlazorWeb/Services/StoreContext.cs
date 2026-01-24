using Blazored.LocalStorage;

namespace SellerInventory.Client.BlazorWeb.Services;

public class StoreContext : IStoreContext
{
    private readonly ILocalStorageService _localStorage;
    private const string StoreIdKey = "storeId";
    private const string StoreNameKey = "storeName";

    private Guid? _currentStoreId;
    private string? _currentStoreName;
    private bool _isInitialized;

    public Guid? CurrentStoreId => _currentStoreId;
    public string? CurrentStoreName => _currentStoreName;
    public bool HasStoreContext => _currentStoreId.HasValue;

    public event Action? OnStoreContextChanged;

    public StoreContext(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            var storeIdStr = await _localStorage.GetItemAsync<string>(StoreIdKey);
            if (!string.IsNullOrEmpty(storeIdStr) && Guid.TryParse(storeIdStr, out var storeId))
            {
                _currentStoreId = storeId;
            }

            _currentStoreName = await _localStorage.GetItemAsync<string>(StoreNameKey);
        }
        catch
        {
            // Ignore errors during initialization (e.g., prerendering)
        }

        _isInitialized = true;
    }

    public async Task SetStoreContextAsync(Guid? storeId, string? storeName)
    {
        _currentStoreId = storeId;
        _currentStoreName = storeName;

        if (storeId.HasValue)
        {
            await _localStorage.SetItemAsync(StoreIdKey, storeId.Value.ToString());
            await _localStorage.SetItemAsync(StoreNameKey, storeName ?? "");
        }
        else
        {
            await _localStorage.RemoveItemAsync(StoreIdKey);
            await _localStorage.RemoveItemAsync(StoreNameKey);
        }

        OnStoreContextChanged?.Invoke();
    }

    public async Task ClearStoreContextAsync()
    {
        await SetStoreContextAsync(null, null);
    }
}
