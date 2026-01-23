using FluentValidation;
using SellerInventory.Application.DTOs.User;

namespace SellerInventory.Application.Validators;

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                       r.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                       r.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be 'Admin', 'Manager', or 'Staff'");
    }
}
