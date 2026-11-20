using Qeq.Domain.Common;
using Qeq.Domain.Enums;

namespace Qeq.Domain.Entities;

public class ChatMessage : AuditableBaseEntity
{
    public Guid ConversationId { get; set; }
    public MessageRole Role { get; set; }
    public required string Content { get; set; }

    // Khóa idempotency: gửi lại cùng RequestId không tạo bản ghi trùng.
    public required string RequestId { get; set; }
}