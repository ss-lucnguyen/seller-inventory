using FluentValidation;
using SellerInventory.Application.DTOs.User;

namespace SellerInventory.Application.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

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
