namespace SellerInventer.Application.DTOs.User;

public record UserListDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
