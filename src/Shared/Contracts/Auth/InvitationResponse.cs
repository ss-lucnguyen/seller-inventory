namespace SellerInventory.Shared.Contracts.Auth;

public record InvitationResponse(
    Guid Id,
    string Email,
    string Role,
    string StoreName,
    DateTime ExpiresAt,
    bool IsUsed
);
