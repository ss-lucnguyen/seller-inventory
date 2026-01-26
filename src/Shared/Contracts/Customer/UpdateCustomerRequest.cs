namespace SellerInventory.Shared.Contracts.Customer;

public record UpdateCustomerRequest(
    string Name,
    string Gender,
    string? Mobile,
    string? Address,
    bool IsActive
);
