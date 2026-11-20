using Qeq.Application.Agents;
using Qeq.Application.DTOs.Chat;
using Qeq.Application.DTOs.Llm;
using Qeq.Application.Tools;
using Qeq.Application.Tests.Fakes;
using Xunit;

namespace Qeq.Application.Tests;

public class SecretaryAgentTests
{
    private static LlmReply Text(string content) => new(content, Array.Empty<LlmToolCall>(), "fake-model");

    private static LlmReply Call(string id, string name, string args)
        => new(null, new[] { new LlmToolCall(id, name, args) }, "fake-model");

    private static SecretaryAgent Build(FakeLlmClient llm, FakeTool tool)
    {
        var registry = new ToolRegistry(new[] { tool });
        return new SecretaryAgent(llm, new ToolExecutor(registry), registry, new FixedDateTimeService());
    }

    private static async Task<List<ChatEvent>> Collect(SecretaryAgent agent, string text)
    {
        var events = new List<ChatEvent>();
        await foreach (var ev in agent.RunAsync(Array.Empty<LlmMessage>(), text))
            events.Add(ev);
        return events;
    }

    [Fact]
    public async Task KhongCoToolCall_PhatMotTextMessageContent()
    {
        var llm = new FakeLlmClient(Text("Chào bạn"));
        var agent = Build(llm, new FakeTool("add_transaction", _ => "{}"));

        var events = await Collect(agent, "chào");

        var text = Assert.Single(events.OfType<TextMessageContentEvent>());
        Assert.Equal("Chào bạn", text.Delta);
        Assert.Single(llm.Calls);
    }

    [Fact]
    public async Task MotToolCall_RoiTraLoi_PhatDuStartEndText()
    {
        var llm = new FakeLlmClient(
            Call("c1", "add_transaction", """{"amount":35000}"""),
            Text("Đã ghi 35k"));
        var tool = new FakeTool("add_transaction", _ => """{"id":"abc"}""");
        var agent = Build(llm, tool);

        var events = await Collect(agent, "trưa nay ăn 35k");

        Assert.Equal(3, events.Count);
        Assert.IsType<ToolCallStartEvent>(events[0]);
        Assert.IsType<ToolCallEndEvent>(events[1]);
        Assert.IsType<TextMessageContentEvent>(events[2]);
        Assert.Equal("""{"amount":35000}""", tool.Received.Single());

        var round2 = llm.Calls[1];
        Assert.Contains(round2, m => m.Role == "assistant" && m.ToolCalls!.Count == 1);
        Assert.Contains(round2, m => m.Role == "tool" && m.ToolCallId == "c1" && m.Content == """{"id":"abc"}""");
    }

    [Fact]
    public async Task ToolTraLoiLoi_AgentVanDiTiepVaGuiLoiChoModel()
    {
        var llm = new FakeLlmClient(
            Call("c1", "add_transaction", """{"amount":-5}"""),
            Text("Số tiền không hợp lệ, bạn nhập lại nhé"));
        var tool = new FakeTool("add_transaction", _ => """{"error":"validation_failed","message":"Amount: phải > 0"}""");
        var agent = Build(llm, tool);

        var events = await Collect(agent, "ghi -5k");

        Assert.Equal(2, llm.Calls.Count);
        Assert.Contains(llm.Calls[1], m => m.Role == "tool" && m.Content!.Contains("validation_failed"));
        Assert.IsType<TextMessageContentEvent>(events[^1]);
    }

    [Fact]
    public async Task LuonGoiTool_ChamTran5Vong_PhatRunError()
    {
        var llm = new FakeLlmClient(
            Call("c1", "add_transaction", "{}"),
            Call("c2", "add_transaction", "{}"),
            Call("c3", "add_transaction", "{}"),
            Call("c4", "add_transaction", "{}"),
            Call("c5", "add_transaction", "{}"),
            Call("c6", "add_transaction", "{}"));
        var agent = Build(llm, new FakeTool("add_transaction", _ => "{}"));

        var events = await Collect(agent, "ghi đi");

        Assert.Equal(5, llm.Calls.Count);
        var error = Assert.IsType<RunErrorEvent>(events[^1]);
        Assert.Contains("5", error.Message);
        Assert.Contains("add_transaction", error.Message);
    }

    [Fact]
    public async Task SystemPrompt_CoGioHienTai()
    {
        var llm = new FakeLlmClient(Text("ok"));
        var agent = Build(llm, new FakeTool("add_transaction", _ => "{}"));

        await Collect(agent, "chào");

        var system = llm.Calls[0].Single(m => m.Role == "system");
        Assert.Contains("2026-11-19", system.Content);
    }
}