using System.Diagnostics;

namespace GameOfLife.Api.Middleware;

public sealed class MetricsMiddleware
(
    RequestDelegate _next,
    GameOfLife.Api.Services.IMetricsService _metrics
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _metrics.ObserveRequestDuration(sw.Elapsed.TotalSeconds);
        }
    }
}
