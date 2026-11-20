using FluentValidation;

namespace Qeq.Application.Features.Expenses.Queries.GetTransactions;

public class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber bắt đầu từ 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize từ 1 đến 100.");

        RuleFor(x => x.Category)
            .IsInEnum().When(x => x.Category.HasValue).WithMessage("Nhóm chi tiêu không hợp lệ.");
    }
}