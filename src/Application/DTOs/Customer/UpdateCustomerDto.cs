using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.DTOs.Customer;

public record UpdateCustomerDto(
    string Name,
    Gender Gender,
    string? Mobile,
    string? Address,
    bool IsActive
);
