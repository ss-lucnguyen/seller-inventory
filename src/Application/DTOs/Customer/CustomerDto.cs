using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.DTOs.Customer;

public record CustomerDto(
    Guid Id,
    string Name,
    Gender Gender,
    string? Mobile,
    string? AccountNumber,
    string? Address,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
