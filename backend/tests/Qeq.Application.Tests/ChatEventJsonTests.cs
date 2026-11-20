using System.Text.Json;
using Qeq.Application.DTOs.Chat;
using Xunit;

namespace Qeq.Application.Tests;

public class ChatEventJsonTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void TextMessageContent_Serialize_DungTenSuKienTrongContracts()
    {
        ChatEvent ev = new TextMessageContentEvent("m1", "Đã ghi");

        var json = JsonSerializer.Serialize(ev, Options);

        Assert.Contains("\"type\":\"TEXT_MESSAGE_CONTENT\"", json);
        Assert.Contains("\"messageId\":\"m1\"", json);
        Assert.Contains("\"delta\":\"", json);
    }

    [Fact]
    public void ToolCallStart_Serialize_CoToolCallIdVaToolName()
    {
        ChatEvent ev = new ToolCallStartEvent("c1", "add_transaction");

        var json = JsonSerializer.Serialize(ev, Options);

        Assert.Contains("\"type\":\"TOOL_CALL_START\"", json);
        Assert.Contains("\"toolCallId\":\"c1\"", json);
        Assert.Contains("\"toolName\":\"add_transaction\"", json);
    }

    [Fact]
    public void ToolCallEnd_Result_LaObjectKhongPhaiChuoi()
    {
        var result = JsonSerializer.Deserialize<JsonElement>("{\"id\":\"abc\"}");
        ChatEvent ev = new ToolCallEndEvent("c1", result);

        var json = JsonSerializer.Serialize(ev, Options);

        Assert.Contains("\"type\":\"TOOL_CALL_END\"", json);
        Assert.Contains("\"result\":{\"id\":\"abc\"}", json);
    }

    [Fact]
    public void RunError_Serialize_CoMessage()
    {
        ChatEvent ev = new RunErrorEvent("kẹt");

        var json = JsonSerializer.Serialize(ev, Options);

        Assert.Contains("\"type\":\"RUN_ERROR\"", json);
        Assert.Contains("\"message\":\"", json);
    }
}