using System.Net;
using System.Text.Json;
using ContentGenerator.Core.DTOs.Common;

namespace ContentGenerator.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse();

        switch (exception)
        {
            case UnauthorizedAccessException:
                response = ApiResponse.ErrorResult("Unauthorized access");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case ArgumentNullException:
                response = ApiResponse.ErrorResult("Invalid request parameters - null value");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case ArgumentException:
                response = ApiResponse.ErrorResult("Invalid request parameters");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case KeyNotFoundException:
                response = ApiResponse.ErrorResult("Resource not found");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case InvalidOperationException:
                response = ApiResponse.ErrorResult("Invalid operation");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            default:
                response = ApiResponse.ErrorResult("An error occurred while processing your request");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}