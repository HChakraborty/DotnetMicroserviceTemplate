using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using ServiceName.Api.Extensions.Application;
using ServiceName.Api.Extensions.Builder;
using ServiceName.Api.Extensions.Services;
using ServiceName.Api.Middlewares;
using ServiceName.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// JWT Auth
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

builder.Services.AddSwagger();

// Layer wiring
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Rate Limiting
builder.Services.AddRateLimit();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<AppDbContext>(tags: ["ready"]);

// Logging
builder.Host
    .AddLoggingConfiguration(builder.Configuration)
    .AddGlobalExceptionHandling();

// Messaging Event Consumers
builder.Services.AddMessagingEvent();

var app = builder.Build();

app.ApplyDatabaseMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceName API v1");
    });
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