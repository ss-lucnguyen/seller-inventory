using FluentValidation;
using SellerInventory.Application.DTOs.Invoice;

namespace SellerInventory.Application.Validators;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order is required");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Due date must be today or in the future")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => x.Notes is not null);
    }
}

public class UpdateInvoicePaymentValidator : AbstractValidator<UpdateInvoicePaymentDto>
{
    public UpdateInvoicePaymentValidator()
    {
        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0).WithMessage("Amount paid cannot be negative");
    }
}
