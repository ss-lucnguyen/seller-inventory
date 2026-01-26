namespace SellerInventory.Shared.Contracts.Customer;

public record CustomerResponse(
    Guid Id,
    string Name,
    string Gender,
    string? Mobile,
    string? AccountNumber,
    string? Address,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
