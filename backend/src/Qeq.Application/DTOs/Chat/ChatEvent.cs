using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qeq.Application.DTOs.Chat;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContentEvent), "TEXT_MESSAGE_CONTENT")]
[JsonDerivedType(typeof(ToolCallStartEvent), "TOOL_CALL_START")]
[JsonDerivedType(typeof(ToolCallEndEvent), "TOOL_CALL_END")]
[JsonDerivedType(typeof(RunErrorEvent), "RUN_ERROR")]
public abstract record ChatEvent;

public record TextMessageContentEvent(string MessageId, string Delta) : ChatEvent;

public record ToolCallStartEvent(string ToolCallId, string ToolName) : ChatEvent;

public record ToolCallEndEvent(string ToolCallId, JsonElement Result) : ChatEvent;

public record RunErrorEvent(string Message) : ChatEvent;