using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.DTOs.Customer;

public record CreateCustomerDto(
    string Name,
    Gender Gender,
    string? Mobile,
    string? Address
);
