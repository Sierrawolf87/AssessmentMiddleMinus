using FluentValidation;

namespace DataProcessor.Features.Messages;

public class ProcessMessageCommandValidator : AbstractValidator<ProcessMessageCommand>
{
    public ProcessMessageCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
    }
}
