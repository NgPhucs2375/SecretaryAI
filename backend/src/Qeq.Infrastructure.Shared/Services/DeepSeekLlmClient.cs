using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Qeq.Application.DTOs.Llm;
using Qeq.Application.Exceptions;
using Qeq.Application.Interfaces;
using Qeq.Domain.Settings;

namespace Qeq.Infrastructure.Shared.Services;

public class DeepSeekLlmClient : ILlmClient
{
    
    // || =============================================== || //
    // || ============ Khai báo & Ctor ================== || //
    // || =============================================== || //
    private readonly HttpClient _http;
    private readonly DeepSeekSettings _settings;

    public DeepSeekLlmClient(HttpClient http, IOptions<DeepSeekSettings> options)
    {
        _http = http;
        _settings = options.Value;
    }

    // || =============================================== || //
    // || ============== Dịch Request  ================== || //
    // || =============================================== || //
        public async Task<LlmReply> ChatAsync(
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<LlmTool> tools,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Chưa cấu hình DeepSeek:ApiKey.");

        var body = new Dictionary<string, object?>
        {
            ["model"] = _settings.Model,
            ["messages"] = messages.Select(m =>
            {
                var msg = new Dictionary<string, object?>
                {
                    ["role"] = m.Role,
                    ["content"] = m.Content
                };

                if (m.ToolCallId is not null)
                    msg["tool_call_id"] = m.ToolCallId;

                if (m.ToolCalls is { Count: > 0 })
                    msg["tool_calls"] = m.ToolCalls.Select(tc => new
                    {
                        id = tc.Id,
                        type = "function",
                        function = new { name = tc.Name, arguments = tc.ArgumentsJson }
                    }).ToArray();

                return msg;
            }).ToArray()
        };

        if (tools.Count > 0)
        {
            body["tools"] = tools.Select(t => new
            {
                type = "function",
                function = new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = JsonSerializer.Deserialize<JsonElement>(t.ParametersJson)
                }
            }).ToArray();
            body["tool_choice"] = "auto";
        }

        var json = JsonSerializer.Serialize(body);
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.Endpoint}/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        
        // || =============================================== || //
        // || ============= Gửi & Bắt lỗi  ================== || //
        // || =============================================== || //
        using var response = await _http.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new LlmProviderException(
                $"DeepSeek trả {(int)response.StatusCode}: {responseJson}",
                (int)response.StatusCode);

        
        
        // || =============================================== || //
        // || ============ Dịch Response ================== || //
        // || =============================================== || //
        using var doc = JsonDocument.Parse(responseJson);
        var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message");

        string? content = null;
        if (message.TryGetProperty("content", out var contentEl) &&
            contentEl.ValueKind == JsonValueKind.String)
        {
            var text = contentEl.GetString();
            content = string.IsNullOrEmpty(text) ? null : text;
        }

        var calls = new List<LlmToolCall>();
        if (message.TryGetProperty("tool_calls", out var toolCallsEl) &&
            toolCallsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var tc in toolCallsEl.EnumerateArray())
            {
                var fn = tc.GetProperty("function");
                calls.Add(new LlmToolCall(
                    tc.GetProperty("id").GetString()!,
                    fn.GetProperty("name").GetString()!,
                    fn.GetProperty("arguments").GetString()!));
            }
        }

        var model = doc.RootElement.GetProperty("model").GetString() ?? _settings.Model;
        return new LlmReply(content, calls, model);
    }
}