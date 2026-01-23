namespace SellerInventory.Application.DTOs.Category;

public record UpdateCategoryDto(string Name, string? Description, bool IsActive);
