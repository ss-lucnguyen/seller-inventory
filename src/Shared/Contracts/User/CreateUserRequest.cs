namespace SellerInventer.Shared.Contracts.User;

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Role
);
