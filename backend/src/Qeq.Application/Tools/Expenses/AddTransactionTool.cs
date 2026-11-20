using System.Globalization;
using System.Text.Json;
using FluentValidation.Results;
using MediatR;
using Qeq.Application.Features.Expenses.Commands.AddTransaction;
using Qeq.Application.Interfaces;
using Qeq.Domain.Enums;
using ValidationException = Qeq.Application.Exceptions.ValidationException;

namespace Qeq.Application.Tools.Expenses;

public class AddTransactionTool : IQeqTool
{
    // || =============================================== || //
    // || ============== Khai báo & Schema  ================== || //
    // || =============================================== || //

    private readonly IMediator _mediator;

    // || =============================================== || //
    // || ================= Dịch Phiếu ================== || //
    // || =============================================== || //

    public AddTransactionTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Name => "add_transaction";

    public string Description => "Ghi một khoản chi tiêu. Dùng khi người dùng kể đã tiêu tiền (ăn uống, đi lại, hóa đơn...).";

    public string ParametersJson => """
        {
          "type": "object",
          "properties": {
            "amount": { "type": "number", "description": "Số tiền VNĐ, phải lớn hơn 0" },
            "category": { "type": "string", "enum": ["food","transport","education","housing","entertainment","health","other"] },
            "note": { "type": "string", "description": "Ghi chú ngắn, có thể bỏ trống" },
            "occurredAt": { "type": "string", "description": "Thời điểm chi tiêu, ISO 8601, ví dụ 2026-11-19T08:00:00+07:00" }
          },
          "required": ["amount", "category", "occurredAt"]
        }
        """;


    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("amount", out var amountEl) ||
            amountEl.ValueKind != JsonValueKind.Number ||
            !amountEl.TryGetDecimal(out var amount))
            throw Fail("Amount", "amount thiếu hoặc không phải số.");

        if (!root.TryGetProperty("category", out var categoryEl) || categoryEl.ValueKind != JsonValueKind.String)
            throw Fail("Category", "category thiếu hoặc không phải chuỗi.");

        var rawCategory = categoryEl.GetString()!.Trim();

        if (rawCategory.Equals("transportation", StringComparison.OrdinalIgnoreCase))
            rawCategory = "transport";

        if (rawCategory.All(char.IsDigit) ||
            !Enum.TryParse<ExpenseCategory>(rawCategory, ignoreCase: true, out var category))
            throw Fail("Category", $"category '{rawCategory}' không hợp lệ; chỉ nhận food, transport, education, housing, entertainment, health, other.");

        if (!root.TryGetProperty("occurredAt", out var atEl) ||
            atEl.ValueKind != JsonValueKind.String ||
            !DateTimeOffset.TryParse(atEl.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var occurredAt))
            throw Fail("OccurredAt", "occurredAt thiếu hoặc không đúng ISO 8601.");

        string? note = root.TryGetProperty("note", out var noteEl) && noteEl.ValueKind == JsonValueKind.String
            ? noteEl.GetString()
            : null;


    // || =============================================== || //
    // || ============== Gọi UC & Trả kết quả  ================== || //
    // || =============================================== || //

            var response = await _mediator.Send(
            new AddTransactionCommand(amount, category, note, occurredAt), ct);

        return JsonSerializer.Serialize(new { id = response.Data, message = response.Message });
    }

    private static ValidationException Fail(string field, string message)
        => new(new[] { new ValidationFailure(field, $"{field}: {message}") });
}