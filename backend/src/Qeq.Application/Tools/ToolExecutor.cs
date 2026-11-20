using System.Text.Json;
using Qeq.Application.Exceptions;
using Qeq.Application.Interfaces;
using ValidationException = Qeq.Application.Exceptions.ValidationException;

namespace Qeq.Application.Tools;

public class ToolExecutor
{
    private readonly IToolRegistry _registry;

    public ToolExecutor(IToolRegistry registry)
    {
        _registry = registry;
    }

    public async Task<string> ExecuteAsync(string toolName, string argumentsJson, CancellationToken ct = default)
    {
        var tool = _registry.Find(toolName);
        if (tool is null)
            return Error("unknown_tool",
                $"Không có tool '{toolName}'. Tool hiện có: {string.Join(", ", _registry.All.Select(t => t.Name))}.");

        try
        {
            return await tool.ExecuteAsync(argumentsJson, ct);
        }
        catch (ValidationException ex)
        {
            return Error("validation_failed", string.Join(" | ", ex.Errors));
        }
        catch (JsonException)
        {
            return Error("invalid_arguments_json",
                "arguments không phải JSON hợp lệ; gọi lại tool với một JSON object đúng schema.");
        }
        catch (ApiException ex)
        {
            return Error("api_error", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return Error("not_found", ex.Message);
        }
    }

    private static string Error(string code, string message)
        => JsonSerializer.Serialize(new { error = code, message });
}