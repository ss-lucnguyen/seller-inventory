using SellerInventory.Application.DTOs.Store;
using SellerInventory.Application.Interfaces;

namespace SellerInventory.Application.Services;

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IStorageService _storageService;

    public StoreService(IUnitOfWork unitOfWork, ITenantContext tenantContext, IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _storageService = storageService;
    }

    public async Task<StoreDto?> GetCurrentStoreAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            return null;
        }

        var store = await _unitOfWork.Stores.GetByIdAsync(_tenantContext.CurrentStoreId.Value, cancellationToken);
        if (store is null) return null;

        return MapToDto(store);
    }

    public async Task<StoreDto> UpdateStoreAsync(UpdateStoreDto dto, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("No store context available");
        }

        if (_tenantContext.CurrentUserRole != Domain.Enums.UserRole.Manager && !_tenantContext.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("Only managers can update store settings");
        }

        var store = await _unitOfWork.Stores.GetByIdAsync(_tenantContext.CurrentStoreId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Store not found");

        store.Name = dto.Name;
        store.Location = dto.Location;
        store.Address = dto.Address;
        store.Industry = dto.Industry;
        store.Description = dto.Description;
        store.ContactEmail = dto.ContactEmail;
        store.ContactPhone = dto.ContactPhone;
        store.Currency = dto.Currency;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(store);
    }

    public async Task<string?> UpdateLogoAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("No store context available");
        }

        if (_tenantContext.CurrentUserRole != Domain.Enums.UserRole.Manager && !_tenantContext.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("Only managers can update store logo");
        }

        var store = await _unitOfWork.Stores.GetByIdAsync(_tenantContext.CurrentStoreId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Store not found");

        // Delete old logo if exists
        if (!string.IsNullOrEmpty(store.LogoUrl))
        {
            await _storageService.DeleteImageAsync(store.LogoUrl, cancellationToken);
        }

        // Upload new logo
        var contentType = GetContentType(fileName);
        var logoUrl = await _storageService.UploadImageAsync(imageStream, $"stores/{store.Id}/{fileName}", contentType, cancellationToken);

        store.LogoUrl = logoUrl;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return logoUrl;
    }

    private static StoreDto MapToDto(Domain.Entities.Store store) =>
        new(
            store.Id,
            store.Name,
            store.Slug,
            store.Location,
            store.Address,
            store.Industry,
            store.LogoUrl,
            store.Description,
            store.ContactEmail,
            store.ContactPhone,
            store.Currency,
            store.IsActive,
            store.SubscriptionStatus,
            store.SubscriptionExpiresAt,
            store.CreatedAt,
            store.UpdatedAt
        );

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
