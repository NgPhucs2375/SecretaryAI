using Qeq.Domain.Common;
using Qeq.Domain.Enums;

namespace Qeq.Domain.Entities;

public class PlanTask : AuditableBaseEntity
{
    public required string Title { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public int? EstimatedMinutes { get; set; }
    public TaskImportance Importance { get; set; } = TaskImportance.Normal;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public DateTimeOffset? CompletedAt { get; set; }

    public bool IsOverdue => Status == TaskItemStatus.Pending && DueAt < DateTimeOffset.UtcNow;
}