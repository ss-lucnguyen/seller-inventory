using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.DTOs.Store;

public record StoreDto(
    Guid Id,
    string Name,
    string Slug,
    string? Location,
    string? Address,
    string? Industry,
    string? LogoUrl,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? Currency,
    bool IsActive,
    SubscriptionStatus SubscriptionStatus,
    DateTime? SubscriptionExpiresAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
