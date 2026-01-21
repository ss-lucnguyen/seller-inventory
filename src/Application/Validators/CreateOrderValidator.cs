using FluentValidation;
using SellerInventer.Application.DTOs.Order;

namespace SellerInventer.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");
        });

        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax cannot be negative");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => x.Notes is not null);
    }
}
