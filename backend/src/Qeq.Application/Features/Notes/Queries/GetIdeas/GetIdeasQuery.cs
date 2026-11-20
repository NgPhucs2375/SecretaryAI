using MediatR;
using Microsoft.EntityFrameworkCore;
using Qeq.Application.DTOs.Notes;
using Qeq.Application.Interfaces;
using Qeq.Application.Wrappers;
using Qeq.Domain.Entities;

namespace Qeq.Application.Features.Notes.Queries.GetIdeas;

public record GetIdeasQuery(
    int PageNumber = 1,
    int PageSize = 20 ): IRequest<Response<PagedList<IdeaNoteDto>>>;


public class GetIdeasQueryHandler : IRequestHandler<GetIdeasQuery, Response<PagedList<IdeaNoteDto>>>
{
    private readonly IApplicationDbContext _context;
    
    public GetIdeasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<PagedList<IdeaNoteDto>>> Handle(GetIdeasQuery request, CancellationToken cancellationToken)
    {
        // không cần phải theo dõi để query nhanh hơn từ IApplicationDbContext
        var query = _context.IdeaNotes.AsNoTracking();
    
        var paged = await PagedList<IdeaNote>.ToPagedListAsync(
            query.OrderByDescending(t => t.CreatedAt),
            request.PageNumber, request.PageSize, cancellationToken);

        var dtos = paged
            .Select(t => new IdeaNoteDto(
                t.Id,
                t.Content,
                t.CreatedAt
            ))
            .ToList();

        var result = new PagedList<IdeaNoteDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
        return new Response<PagedList<IdeaNoteDto>>(result);
    }
}