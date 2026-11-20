namespace Qeq.Application.DTOs.Expenses;

// Khớp TransactionDto trong packages/contracts (web + mobile dùng chung).
public record TransactionDto(
    Guid Id,
    decimal Amount,
    string Category,
    string? Note,
    DateTimeOffset OccurredAt);