 using Qeq.Application.DTOs.Llm;
 
namespace Qeq.Application.Interfaces;

public interface ILlmClient
{
    Task<LlmReply> ChatAsync(IReadOnlyList<LlmMessage> messages, IReadOnlyList<LlmTool> tools, CancellationToken cancellationToken = default);
}