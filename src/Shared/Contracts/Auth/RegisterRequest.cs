namespace SellerInventer.Shared.Contracts.Auth;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Role
);
