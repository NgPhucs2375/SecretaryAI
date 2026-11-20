using Qeq.Application.Exceptions;
using Qeq.Application.Wrappers;
using System.Net;
using System.Text.Json;

namespace Qeq.Api.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;

            // Response đã bắt đầu gửi (vd SSE đang stream): không đổi status/body được, chỉ log.
            if (response.HasStarted)
            {
                _logger.LogError(error, "Lỗi sau khi response đã bắt đầu gửi; không thể ghi error body.");
                return;
            }

            _logger.LogError(error, "Unhandled exception tại {Path}.", context.Request.Path);

            response.ContentType = "application/json";
            var responseModel = new Response<string> { Succeeded = false, Message = error.Message };

            switch (error)
            {
                case ApiException e:
                    response.StatusCode = e.StatusCode > 0 ? e.StatusCode : (int)HttpStatusCode.BadRequest;
                    break;
                case ValidationException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    responseModel.Errors = e.Errors;
                    break;
                case FluentValidation.ValidationException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    responseModel.Errors = e.Errors.Select(f => f.ErrorMessage).Distinct().ToList();
                    break;
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                default:
                    // Không lộ chi tiết nội bộ (DB/SQL/đường dẫn) ra client.
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseModel.Message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
                    break;
            }

            await response.WriteAsync(JsonSerializer.Serialize(responseModel,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}