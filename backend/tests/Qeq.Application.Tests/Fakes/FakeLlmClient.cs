using Qeq.Application.DTOs.Llm;
using Qeq.Application.Interfaces;

namespace Qeq.Application.Tests.Fakes;

public class FakeLlmClient : ILlmClient
{
    private readonly Queue<LlmReply> _replies;

    public List<IReadOnlyList<LlmMessage>> Calls { get; } = new();

    public FakeLlmClient(params LlmReply[] replies)
    {
        _replies = new Queue<LlmReply>(replies);
    }

    public Task<LlmReply> ChatAsync(
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<LlmTool> tools,
        CancellationToken cancellationToken = default)
    {
        Calls.Add(messages);
        return Task.FromResult(_replies.Dequeue());
    }
}