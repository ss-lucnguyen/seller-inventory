using FluentValidation;
using SellerInventory.Application.DTOs.Customer;

namespace SellerInventory.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(100).WithMessage("Customer name must not exceed 100 characters");

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile must not exceed 20 characters")
            .When(x => x.Mobile is not null);

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters")
            .When(x => x.Address is not null);
    }
}
