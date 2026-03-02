using Serilog;

namespace SampleAuthService.Api.Extensions.Services;

public static class LoggingExtensions
{
    public static IHostBuilder AddLoggingConfiguration(
        this IHostBuilder host,
        IConfiguration config)
    {
        // Logging behavior (level, sinks, formats) is driven by appsettings file or environment variables (e.g., in production)
        // so users of this template can change it without modifying code.
        // Here in the template, we add logging to a file but in production we have seperate db or other services to store logging details.
        var logFile = $"logs/log-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .WriteTo.File(logFile) // override path
            .Enrich.FromLogContext()
            .CreateLogger();

        host.UseSerilog();

        return host;
    }
}
