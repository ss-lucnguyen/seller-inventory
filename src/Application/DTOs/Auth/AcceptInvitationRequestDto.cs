namespace SellerInventory.Application.DTOs.Auth;

public record AcceptInvitationRequestDto(
    string Token,
    string Username,
    string Password,
    string FullName
);
