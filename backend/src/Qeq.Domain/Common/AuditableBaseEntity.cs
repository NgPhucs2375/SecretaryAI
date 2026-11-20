namespace Qeq.Domain.Common;

public abstract class AuditableBaseEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}