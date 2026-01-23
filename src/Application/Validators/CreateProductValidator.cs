using FluentValidation;
using SellerInventory.Application.DTOs.Product;

namespace SellerInventory.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.SKU)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .When(x => x.SKU is not null);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative");

        RuleFor(x => x.SellPrice)
            .GreaterThan(0).WithMessage("Sell price must be greater than zero");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");
    }
}
