namespace Qeq.Application.DTOs.Planning;

public record PlanTaskDto(
    Guid Id, // Khóa chính
    string Title, //Tiêu đề
    DateTimeOffset? DueAt, // Có thể không có ngày đến hạn
    int? EstimatedMinutes, // Có thể không có thời gian ước lượng
    string Importance, // Mức độ quan trọng (ví dụ: "high", "normal", "low")
    bool Done, // Trạng thái hoàn thành
    bool Overdue // Trạng thái quá hạn
);