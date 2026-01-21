namespace SellerInventer.Application.DTOs.Auth;

public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Role
);
