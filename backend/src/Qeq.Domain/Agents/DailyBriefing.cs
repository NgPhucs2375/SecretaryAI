namespace Qeq.Domain.Agents;

public class DailyBriefing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Date { get; set; }
    public BriefingKind Kind { get; set; }
    public required string Content { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}