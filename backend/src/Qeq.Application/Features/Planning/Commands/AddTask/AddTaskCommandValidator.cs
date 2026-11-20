using FluentValidation;

namespace Qeq.Application.Features.Planning.Commands.AddTask;

public class AddTaskCommandValidator : AbstractValidator<AddTaskCommand>
{
    public AddTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(300).WithMessage("Tiêu đề không được để trống và tối đa 300 ký tự.");

        RuleFor(x => x.EstimatedMinutes)
            .GreaterThan(0).When(x => x.EstimatedMinutes.HasValue).WithMessage("Thời gian ước lượng phải lớn hơn 0 nếu được cung cấp.");

        RuleFor(x => x.Importance)
            .IsInEnum().WithMessage("Mức độ quan trọng không hợp lệ.");
    }
}