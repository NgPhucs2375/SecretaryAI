using MediatR;
using Qeq.Application.Interfaces;
using Qeq.Application.Wrappers;
using Qeq.Domain.Entities;
using Qeq.Domain.Enums;

namespace Qeq.Application.Features.Expenses.Commands.AddTransaction;

public record AddTransactionCommand(
    decimal Amount,
    ExpenseCategory Category,
    string? Note,
    DateTimeOffset OccurredAt) : IRequest<Response<Guid>>;

public class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, Response<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<Guid>> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Transaction
        {
            Amount = request.Amount,
            Category = request.Category,
            Note = request.Note,
            // Npgsql chỉ ghi được DateTimeOffset offset 0 vào timestamptz.
            OccurredAt = request.OccurredAt.ToUniversalTime()
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return new Response<Guid>(transaction.Id, "Đã ghi khoản chi.");
    }
}