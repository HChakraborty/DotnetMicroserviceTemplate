using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SampleAuthService.Api.Extensions.Application;
using SampleAuthService.Api.Extensions.Services;
using SampleAuthService.Api.Middlewares;
using SampleAuthService.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllersOptions();

// Layer wiring
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

// JWT Auth
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Rate Limiting
builder.Services.AddRateLimit();

// Logging
builder.Host
    .AddLoggingConfiguration(builder.Configuration)
    .AddGlobalExceptionHandling();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<AuthDbContext>(tags: ["ready"]);

// Messaging Event Consumers
builder.Services.AddMessagingEvent();

var app = builder.Build();

app.ApplyDatabaseMigrations();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseMiddleware<HttpErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("global");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.Run();

public partial class Program { }
