namespace Qeq.Application.DTOs.Llm;

public record LlmReply(
    string? Content, // Noi dung cua reply co the la null neu reply la tool call
    IReadOnlyList<LlmToolCall> ToolCalls, // danh sách phiếu tool đã điền
    string Model // Model duoc su dung de tao ra reply
);