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
// In-memory metrics service (Prometheus-compatible exposition endpoint is provided below).
builder.Services.AddSingleton<GameOfLife.Api.Services.IMetricsService, GameOfLife.Api.Services.MetricsService>();

// -----------------------------------------------------------------------
// Pipeline
// -----------------------------------------------------------------------

var app = builder.Build();

// Apply EF Core schema (creates DB file / runs EnsureCreated on first run).
app.Services.ApplyMigrations();

// Metrics middleware should run early to measure all requests.
app.UseMiddleware<GameOfLife.Api.Middleware.MetricsMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseSwaggerDocumentation();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// Expose a simple Prometheus-compatible metrics endpoint.
app.MapGet("/metrics", (GameOfLife.Api.Services.IMetricsService metrics)
    => Results.Text(metrics.GetMetricsText(), "text/plain; version=0.0.4"));

app.Run();

// Make Program accessible for integration test WebApplicationFactory
public partial class Program { }
