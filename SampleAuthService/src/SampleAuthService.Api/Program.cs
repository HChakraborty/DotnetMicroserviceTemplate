using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SampleAuthService.Api.Extensions;
using SampleAuthService.Api.Middlewares;
using SampleAuthService.Infrastructure.Persistence;
using System;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

// Layer wiring
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Auth
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("global", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuthDbContext>();

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

app.UseRateLimiter();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("global");

app.MapHealthChecks("/health");

app.Run();
