using Qeq.Domain.Common;
namespace Qeq.Domain.Entities;

public class IdeaNote : AuditableBaseEntity
{
    public required string Content { get; set; }

    // Embedding do Gemini sinh ra; chiều 768 khớp model embedding-001 (outputDimensionality=768).
    public float[]? Embedding { get; set; }
}