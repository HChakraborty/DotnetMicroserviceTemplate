using Microsoft.EntityFrameworkCore;
using SampleAuthService.Infrastructure.Persistence;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var db = services.GetRequiredService<AuthDbContext>();

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
