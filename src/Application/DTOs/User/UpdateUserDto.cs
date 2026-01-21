namespace SellerInventer.Application.DTOs.User;

public record UpdateUserDto(
    string Email,
    string FullName,
    string Role,
    bool IsActive
);
