namespace SellerInventory.Application.DTOs.Auth;

public record InviteUserRequestDto(
    string Email,
    string Role // Staff or Manager only
);
