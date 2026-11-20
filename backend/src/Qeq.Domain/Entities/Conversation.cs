using Qeq.Domain.Common;
namespace Qeq.Domain.Entities;

public class Conversation : AuditableBaseEntity
{
    public required string Title { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}