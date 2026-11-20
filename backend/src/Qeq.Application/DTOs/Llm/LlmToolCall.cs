namespace Qeq.Application.DTOs.Llm;

public record LlmToolCall(
    string Id, // Id cua tool call
    string Name, // Ten cua tool call
    string ArgumentsJson // Doi tuong json chua cac doi so truyen vao tool call
);