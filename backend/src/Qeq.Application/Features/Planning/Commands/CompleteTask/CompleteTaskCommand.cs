using MediatR;
using Qeq.Application.Interfaces;
using Qeq.Application.Wrappers;
using Microsoft.EntityFrameworkCore;
using Qeq.Domain.Enums;

namespace Qeq.Application.Features.Planning.Commands.CompleteTask;

public record CompleteTaskCommand(Guid Id): IRequest<Response<bool>>;

public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, Response<bool>>
{
    private readonly IApplicationDbContext _context;
    
    public CompleteTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<bool>> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException("Không tìm thấy công việc.");
        }

        task.Status = TaskItemStatus.Done;
        task.CompletedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new Response<bool>(true, "Công việc đã được đánh dấu là hoàn thành.");
    }
}