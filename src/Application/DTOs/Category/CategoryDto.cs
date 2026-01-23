namespace SellerInventory.Application.DTOs.Category;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
