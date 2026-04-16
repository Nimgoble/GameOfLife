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

// -----------------------------------------------------------------------
// Pipeline
// -----------------------------------------------------------------------

var app = builder.Build();

// Apply EF Core schema (creates DB file / runs EnsureCreated on first run).
app.Services.ApplyMigrations();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseSwaggerDocumentation();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program accessible for integration test WebApplicationFactory
public partial class Program { }
