namespace Qeq.Application.DTOs.Llm;

public record LlmMessage(
    string Role, // Vai tro cua nguoi gui tin nhan (user,assitant,system)
    string? Content, // Noi dung cua Message 
    string? ToolCallId = null,
    IReadOnlyList<LlmToolCall>? ToolCalls = null);