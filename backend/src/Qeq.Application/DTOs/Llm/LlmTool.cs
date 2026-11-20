namespace Qeq.Application.DTOs.Llm;

public record LlmTool(
    string Name, // Ten cua tool call
    string Description, //  Mo ta cua tool call
    string ParametersJson //  schema (bản mô tả khuôn)
);