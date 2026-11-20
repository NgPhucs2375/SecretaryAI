namespace Qeq.Application.DTOs.Chat;

public record SendMessageRequest(Guid ConversationId, string RequestId, string Text);