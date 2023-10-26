using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
    {
        public DeleteOrderCommandValidator()
        {
            RuleFor(i => i.Id)
                .NotEmpty().WithMessage("{Id} is required")
                .NotNull();
        }
    }
}
