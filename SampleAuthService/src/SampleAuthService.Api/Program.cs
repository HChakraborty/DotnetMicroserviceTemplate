using SampleAuthService.Api.Extensions;
using SampleAuthService.Api.Middlewares;
using SampleAuthService.Infrastructure.Persistence;

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

builder.Services.AddRateLimit();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuthDbContext>();

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

app.UseRateLimiter();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("global");

app.MapHealthChecks("/health");

app.Run();
