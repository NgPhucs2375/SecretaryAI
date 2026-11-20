using FluentValidation;

namespace Qeq.Application.Features.Expenses.Commands.AddTransaction;

public class AddTransactionCommandValidator : AbstractValidator<AddTransactionCommand>
{
    public AddTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Số tiền phải lớn hơn 0.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Nhóm chi tiêu không hợp lệ.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Ghi chú tối đa 500 ký tự.");
    }
}