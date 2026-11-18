namespace Qeq.Domain.Notes;

public class IdeaNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Embedding do Gemini sinh ra; chiều 768 khớp model embedding-001 (outputDimensionality=768).
    public float[]? Embedding { get; set; }
}