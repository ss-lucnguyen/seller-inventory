namespace SellerInventory.Shared.Contracts.Auth;

public record AcceptInvitationRequest(
    string Token,
    string Username,
    string Password,
    string FullName
);
