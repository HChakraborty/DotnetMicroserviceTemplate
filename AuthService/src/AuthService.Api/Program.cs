using Microsoft.EntityFrameworkCore;
using SampleAuthService.Extensions;
using SampleAuthService.Infrastructure.Persistence;
using ServiceName.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Layer wiring
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Auth
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// 🔹 Auto-migrate DB on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AuthDbContext>();

    const int maxRetries = 5;

    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Auth DB migration successful.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Auth DB not ready. Retry {Attempt}/{Max}",
                i,
                maxRetries);

            if (i == maxRetries)
                throw;

            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
