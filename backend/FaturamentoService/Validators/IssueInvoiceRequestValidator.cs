using FluentValidation;
using FaturamentoService.DTOs;

namespace FaturamentoService.Validators;

public class IssueInvoiceRequestValidator : AbstractValidator<IssueInvoiceRequest>
{
    public IssueInvoiceRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("The invoice must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than zero");

            item.RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than zero");
        });
    }
}
