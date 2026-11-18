namespace Qeq.Domain.Agents;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public MessageRole Role { get; set; }
    public required string Content { get; set; }

    // Khóa idempotency: gửi lại cùng RequestId không tạo bản ghi trùng.
    public required string RequestId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}