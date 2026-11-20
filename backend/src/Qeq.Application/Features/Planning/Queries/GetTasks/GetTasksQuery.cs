using MediatR;
using Microsoft.EntityFrameworkCore;
using Qeq.Application.DTOs.Planning;
using Qeq.Application.Interfaces;
using Qeq.Application.Wrappers;
using Qeq.Domain.Entities;
using Qeq.Domain.Enums;

namespace Qeq.Application.Features.Planning.Queries.GetTasks;

public record GetTasksQuery(
    TaskItemStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20 ): IRequest<Response<PagedList<PlanTaskDto>>>;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, Response<PagedList<PlanTaskDto>>>
{
    private readonly IApplicationDbContext _context;
    
    public GetTasksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<PagedList<PlanTaskDto>>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        // không cần phải theo dõi để query nhanh hơn từ IApplicationDbContext
        var query = _context.Tasks.AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        var paged = await PagedList<PlanTask>.ToPagedListAsync(
            query.OrderBy(t => t.DueAt == null).ThenBy(t => t.DueAt),
            request.PageNumber, request.PageSize, cancellationToken);

        var dtos = paged
            .Select(t => new PlanTaskDto(
                t.Id,
                t.Title,
                t.DueAt,
                t.EstimatedMinutes,
                t.Importance.ToString().ToLowerInvariant(),
                t.Status == TaskItemStatus.Done,
                t.IsOverdue))
            .ToList();

        var result = new PagedList<PlanTaskDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
        return new Response<PagedList<PlanTaskDto>>(result);
    }
}