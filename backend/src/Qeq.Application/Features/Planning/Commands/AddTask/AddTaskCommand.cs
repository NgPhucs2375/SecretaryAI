using MediatR;
using Qeq.Application.Interfaces;
using Qeq.Application.Wrappers;
using Qeq.Domain.Entities;
using Qeq.Domain.Enums;

namespace Qeq.Application.Features.Planning.Commands.AddTask;

public record AddTaskCommand(
    string Title,
    DateTimeOffset? DueAt,
    int? EstimatedMinutes,
    TaskImportance Importance = TaskImportance.Normal) : IRequest<Response<Guid>>;

public class AddTaskCommandHandler: IRequestHandler<AddTaskCommand, Response<Guid>>
{
    private readonly IApplicationDbContext _context;
    
    public AddTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<Guid>> Handle(AddTaskCommand request,CancellationToken cancellationToken)
    {
        var planTask = new PlanTask
        {
            Title = request.Title,
            DueAt = request.DueAt?.ToUniversalTime() ,
            EstimatedMinutes = request.EstimatedMinutes,
            Importance = request.Importance,
        };

        _context.Tasks.Add(planTask);
        await _context.SaveChangesAsync(cancellationToken);

        return new Response<Guid>(planTask.Id, "Đã thêm công việc.");
    }
}