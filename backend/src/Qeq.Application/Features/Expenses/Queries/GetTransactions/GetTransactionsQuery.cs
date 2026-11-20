using MediatR;
using Microsoft.EntityFrameworkCore;
using Qeq.Application.DTOs.Expenses;
using Qeq.Application.Interfaces;
using Qeq.Application.Wrappers;
using Qeq.Domain.Entities;
using Qeq.Domain.Enums;

namespace Qeq.Application.Features.Expenses.Queries.GetTransactions;

public record GetTransactionsQuery(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    ExpenseCategory? Category = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Response<PagedList<TransactionDto>>>;

public class GetTransactionsQueryHandler
    : IRequestHandler<GetTransactionsQuery, Response<PagedList<TransactionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<PagedList<TransactionDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions.AsNoTracking();

        if (request.From.HasValue)
            query = query.Where(t => t.OccurredAt >= request.From.Value.ToUniversalTime());
        if (request.To.HasValue)
            query = query.Where(t => t.OccurredAt <= request.To.Value.ToUniversalTime());
        if (request.Category.HasValue)
            query = query.Where(t => t.Category == request.Category.Value);

        var paged = await PagedList<Transaction>.ToPagedListAsync(
            query.OrderByDescending(t => t.OccurredAt),
            request.PageNumber, request.PageSize, cancellationToken);

        var dtos = paged
            .Select(t => new TransactionDto(t.Id, t.Amount, t.Category.ToString().ToLowerInvariant(), t.Note, t.OccurredAt))
            .ToList();

        var result = new PagedList<TransactionDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
        return new Response<PagedList<TransactionDto>>(result);
    }
}