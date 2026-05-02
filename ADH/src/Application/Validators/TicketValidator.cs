using ADH.Core.Entities;
using FluentValidation;

namespace ADH.Application.Validators;

public class TicketValidator : AbstractValidator<Ticket>
{
    public TicketValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
