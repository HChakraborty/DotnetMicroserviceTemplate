using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace SampleAuthService.Api.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    // Global middleware to handle exceptions and return standardized error responses
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = GetTitle(statusCode),
            Detail = _env.IsDevelopment()
                ? exception.Message
                : "An error occurred while processing your request.",
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status.Value;

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string GetTitle(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.NotFound => "Resource Not Found",
            HttpStatusCode.Unauthorized => "Unauthorized",
            _ => "Internal Server Error"
        };
    }
}
