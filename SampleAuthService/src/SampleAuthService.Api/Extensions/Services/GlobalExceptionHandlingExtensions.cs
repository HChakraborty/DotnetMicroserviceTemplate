using Serilog;

namespace SampleAuthService.Api.Extensions.Services;

public static class GlobalExceptionHandlingExtensions
{
    public static IHostBuilder AddGlobalExceptionHandling(
        this IHostBuilder host)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = (Exception)args.ExceptionObject;

            Log.Fatal(
                exception,
                "Unhandled process-level exception");
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Log.Fatal(
                args.Exception,
                "Unobserved task exception");

            args.SetObserved();
        };

        return host;
    }
}
