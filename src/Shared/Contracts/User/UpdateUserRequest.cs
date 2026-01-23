namespace SellerInventory.Shared.Contracts.User;

public record UpdateUserRequest(
    string Email,
    string FullName,
    string Role,
    bool IsActive
);
