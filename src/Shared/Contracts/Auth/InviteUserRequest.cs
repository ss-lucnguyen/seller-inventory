namespace SellerInventory.Shared.Contracts.Auth;

public record InviteUserRequest(
    string Email,
    string Role
);
