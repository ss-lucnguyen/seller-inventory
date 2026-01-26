namespace SellerInventory.Shared.Contracts.Customer;

public record CreateCustomerRequest(
    string Name,
    string Gender,
    string? Mobile,
    string? Address
);
