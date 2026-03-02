using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ServiceName.Api.Middlewares;

public class HttpErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public HttpErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<HttpErrorHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    // Handles unhandled exceptions from the HTTP pipeline and produces
    // standardized ProblemDetails responses with structured logging.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;

            var statusCode = MapStatusCode(ex);

            LogException(context, ex, statusCode, traceId);

            await HandleExceptionAsync(context, ex, statusCode, traceId);
        }
    }

    private void LogException(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string traceId)
    {
        var message =
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}";

        // Use different log levels depending on severity
        switch (statusCode)
        {
            case HttpStatusCode.BadRequest:
            case HttpStatusCode.NotFound:
                _logger.LogWarning(
                    exception,
                    message,
                    traceId,
                    context.Request.Path,
                    context.Request.Method);
                break;

            case HttpStatusCode.Unauthorized:
                _logger.LogInformation(
                    exception,
                    message,
                    traceId,
                    context.Request.Path,
                    context.Request.Method);
                break;

            default:
                _logger.LogError(
                    exception,
                    message,
                    traceId,
                    context.Request.Path,
                    context.Request.Method);
                break;
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string traceId)
    {
        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = GetTitle(statusCode),
            Detail = _env.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred.",
            Instance = context.Request.Path
        };

        // Allows log correlation between client response and server logs
        problem.Extensions["traceId"] = traceId;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status.Value;

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static HttpStatusCode MapStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };
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
