using FluentValidation;
using EstoqueService.DTOs;

namespace EstoqueService.Validators;

public class StockWithdrawalRequestValidator : AbstractValidator<StockWithdrawalRequest>
{
    public StockWithdrawalRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("The items list cannot be empty");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than zero");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");
        });
    }
}
