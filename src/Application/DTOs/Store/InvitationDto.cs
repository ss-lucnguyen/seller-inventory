using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.DTOs.Store;

public record InvitationDto(
    Guid Id,
    string Email,
    UserRole Role,
    string StoreName,
    DateTime ExpiresAt,
    bool IsUsed
);
