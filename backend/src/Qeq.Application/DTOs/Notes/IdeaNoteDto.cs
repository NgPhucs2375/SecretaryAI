namespace Qeq.Application.DTOs.Notes;

public record IdeaNoteDto(
    Guid Id, // Khoa chinhh
    string Content, // Noi dung
    DateTimeOffset CreatedAt // Ngay tao
);