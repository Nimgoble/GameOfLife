using GameOfLife.Api.Extensions;
using GameOfLife.Api.Middleware;
using GameOfLife.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Services
// -----------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructure
(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=gameoflife.db"
);

builder.Services.AddHealthChecks();
// Configure CORS policies from configuration. Use specific allowed origins
// in production and a development policy for local front-ends. Both policies
// are registered if corresponding configuration is present; nothing is
// enabled by default to avoid accidental wide-open CORS in production.
var corsSection = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var devOrigins = corsSection.GetSection("DevelopmentAllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var allowCredentials = corsSection.GetValue<bool?>("AllowCredentials") ?? false;

builder.Services.AddCors(options =>
{
    if (allowedOrigins.Length > 0)
    {
        options.AddPolicy("DefaultCors", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            if (allowCredentials)
                policy.AllowCredentials();
        });
    }

    if (devOrigins.Length > 0)
    {
        options.AddPolicy("LocalDev", policy =>
        {
            policy.WithOrigins(devOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            if (allowCredentials)
                policy.AllowCredentials();
        });
    }
});
// In-memory metrics service (Prometheus-compatible exposition endpoint is provided below).
builder.Services.AddSingleton<GameOfLife.Api.Services.IMetricsService, GameOfLife.Api.Services.MetricsService>();

// -----------------------------------------------------------------------
// Pipeline
// -----------------------------------------------------------------------

var app = builder.Build();

// Log configured CORS origins at startup to help verify runtime configuration
// (useful when troubleshooting CORS in production hosts like Render).
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (allowedOrigins.Length > 0)
    logger.LogInformation("CORS DefaultAllowedOrigins: {Origins}", string.Join(',', allowedOrigins));
else
    logger.LogInformation("CORS DefaultAllowedOrigins: (none)");

if (devOrigins.Length > 0)
    logger.LogInformation("CORS DevelopmentAllowedOrigins: {Origins}", string.Join(',', devOrigins));
else
    logger.LogInformation("CORS DevelopmentAllowedOrigins: (none)");

logger.LogInformation("CORS AllowCredentials: {AllowCredentials}", allowCredentials);

// Apply EF Core schema (creates DB file / runs EnsureCreated on first run).
app.Services.ApplyMigrations();

// Metrics middleware should run early to measure all requests.
app.UseMiddleware<GameOfLife.Api.Middleware.MetricsMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseSwaggerDocumentation();

app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    // Enable CORS policy for local development front-ends.
    app.UseCors("LocalDev");
}
else
{
    // In non-development environments, enable the configured default policy
    // only if allowed origins are provided (avoid enabling a permissive policy
    // with no configured origins).
    if (allowedOrigins.Length > 0)
        app.UseCors("DefaultCors");
}
app.MapControllers();
app.MapHealthChecks("/health");

// Expose a simple Prometheus-compatible metrics endpoint.
app.MapGet("/metrics", (GameOfLife.Api.Services.IMetricsService metrics)
    => Results.Text(metrics.GetMetricsText(), "text/plain; version=0.0.4"));

app.Run();

// Make Program accessible for integration test WebApplicationFactory
public partial class Program { }
