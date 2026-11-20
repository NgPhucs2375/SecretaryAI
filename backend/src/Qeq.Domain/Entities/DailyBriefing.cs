using Qeq.Domain.Common;
using Qeq.Domain.Enums;

namespace Qeq.Domain.Entities;
//báo cáo hàng ngày
public class DailyBriefing : AuditableBaseEntity
{
    public DateOnly Date { get; set; }
    // Loại bản tóm tắt: 
    public BriefingKind Kind { get; set; }
    public required string Content { get; set; }
    public bool IsRead { get; set; }
}