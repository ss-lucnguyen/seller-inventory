namespace SellerInventer.Shared.Contracts.Category;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
