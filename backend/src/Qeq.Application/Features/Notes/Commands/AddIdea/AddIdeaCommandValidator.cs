using FluentValidation;

namespace Qeq.Application.Features.Notes.Commands.AddIdea;

public class AddIdeaCommandValidator : AbstractValidator<AddIdeaCommand>
{
    public AddIdeaCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung ý tưởng không được để trống.")
            .MaximumLength(4000).WithMessage("Nội dung ý tưởng không được vượt quá 4000 ký tự.");
    }
}