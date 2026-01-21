namespace SellerInventer.Shared.Contracts.User;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
