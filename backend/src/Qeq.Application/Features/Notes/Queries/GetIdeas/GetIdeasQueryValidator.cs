using FluentValidation;

namespace Qeq.Application.Features.Notes.Queries.GetIdeas;

public class GetIdeasQueryValidator : AbstractValidator<GetIdeasQuery>
{
    public GetIdeasQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber bắt đầu từ 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize từ 1 đến 100.");
    }
}