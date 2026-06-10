using System.Net;
using System.Text.Json;
using EMS.Application.DTOs.Common;

namespace EMS.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Structured logging — searchable in log files
            _logger.LogError(ex,
                "Unhandled exception. Method: {Method} Path: {Path} User: {User}",
                context.Request.Method,
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anonymous");

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "You are not authorized to perform this action."),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "The requested resource was not found."),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                ex.Message),

            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                ex.Message),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;

        // In development: expose details for debugging
        // In production: only safe message
        var response = _env.IsDevelopment()
            ? ApiResponseDto<object>.Fail(message,
                new List<string> { ex.Message, ex.StackTrace ?? "" })
            : ApiResponseDto<object>.Fail(message);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, options));
    }
}