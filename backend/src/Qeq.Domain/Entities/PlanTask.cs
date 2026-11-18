using Qeq.Domain.Enums;

namespace Qeq.Domain.Entities;

public class PlanTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public int? EstimatedMinutes { get; set; }
    public TaskImportance Importance { get; set; } = TaskImportance.Normal;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public bool IsOverdue => Status == TaskItemStatus.Pending && DueAt < DateTimeOffset.UtcNow;
}