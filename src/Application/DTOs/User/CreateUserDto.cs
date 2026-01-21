namespace SellerInventer.Application.DTOs.User;

public record CreateUserDto(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Role
);
