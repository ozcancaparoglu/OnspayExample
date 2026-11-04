using FluentValidation;

namespace Application.Features.Orders.Create;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderNumber)
            .GreaterThan(0)
            .WithMessage("Order number must be greater than zero.");

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .WithMessage("An order needs at least one item.");

        RuleForEach(x => x.OrderItems).ChildRules(item =>
        {
            item.RuleFor(i => i.Name).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
        });
    }
}
