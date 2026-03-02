using Microsoft.EntityFrameworkCore;
using ServiceName.Infrastructure.Persistence;

namespace ServiceName.Api.Extensions.Builder;

public static class ApplicationBuilderExtensions
{
    // Automatically applies pending migrations at startup
    // Retry logic handles delayed startup of the database
    public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
    {
        if (app.Environment.IsEnvironment("IntegrationTests"))
            return app;

        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var db = services.GetRequiredService<AppDbContext>();

        const int maxRetries = 5;

        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                db.Database.Migrate();
                logger.LogInformation("Database migration successful.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Database not ready. Retry {Attempt}/{Max}",
                    i,
                    maxRetries);

                if (i == maxRetries)
                    throw;

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        return app;
    }
}

