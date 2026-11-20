using FluentValidation;

namespace Qeq.Application.Features.Planning.Queries.GetTasks;

public class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber bắt đầu từ 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize từ 1 đến 100.");
        
        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue).WithMessage("Trạng thái công việc không hợp lệ.");
    }
}