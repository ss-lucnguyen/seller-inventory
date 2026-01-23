namespace SellerInventory.Shared.Contracts.Category;

public record UpdateCategoryRequest(string Name, string? Description, bool IsActive);
