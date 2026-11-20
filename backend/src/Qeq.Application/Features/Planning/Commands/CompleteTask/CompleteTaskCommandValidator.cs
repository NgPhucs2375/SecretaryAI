using FluentValidation;

namespace Qeq.Application.Features.Planning.Commands.CompleteTask;

public class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public CompleteTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id công việc không được để trống.");
            
    }
}