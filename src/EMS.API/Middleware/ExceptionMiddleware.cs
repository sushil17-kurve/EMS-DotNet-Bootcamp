using EMS.Application.DTOs.Common;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

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
            await _next(context); // Run the next middleware/controller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            // Add custom exception types here as you build more features
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "You are not authorized to perform this action."),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "The requested resource was not found."),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                ex.Message),

            // Default: internal server error
            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;

        // In development, include the full stack trace for debugging
        // In production, NEVER expose internal details
        var response = _env.IsDevelopment()
    ? ApiResponseDto<object>.Fail(
        message,
        new List<string> { ex.Message, ex.StackTrace ?? "" })
    : ApiResponseDto<object>.Fail(
        message,
        new List<string>());

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }
}