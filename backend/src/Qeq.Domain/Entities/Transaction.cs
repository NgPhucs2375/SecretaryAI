using Qeq.Domain.Common;
using Qeq.Domain.Enums;

namespace Qeq.Domain.Entities;

public class Transaction : AuditableBaseEntity
{
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}