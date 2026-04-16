using System.Text;
using System.Threading;

namespace GameOfLife.Api.Services;

public sealed class MetricsService : IMetricsService
{
    private long _requests;
    private long _exceptions;
    private double _requestDurationSum;
    private long _requestDurationCount;

    public void IncrementRequest() => Interlocked.Increment(ref _requests);

    public void IncrementException() => Interlocked.Increment(ref _exceptions);

    public void ObserveRequestDuration(double seconds)
    {
        // Note: using lock for updating double and count together to avoid partial updates.
        lock (this)
        {
            _requestDurationSum += seconds;
            _requestDurationCount++;
        }
    }

    public string GetMetricsText()
    {
        var sb = new StringBuilder();

        sb.AppendLine("# HELP gol_requests_total Total number of HTTP requests processed.");
        sb.AppendLine("# TYPE gol_requests_total counter");
        sb.AppendLine($"gol_requests_total {Interlocked.Read(ref _requests)}");

        sb.AppendLine("# HELP gol_exceptions_total Total number of unhandled exceptions.");
        sb.AppendLine("# TYPE gol_exceptions_total counter");
        sb.AppendLine($"gol_exceptions_total {Interlocked.Read(ref _exceptions)}");

        long durationCount;
        double durationSum;
        lock (this)
        {
            durationCount = _requestDurationCount;
            durationSum = _requestDurationSum;
        }

        sb.AppendLine("# HELP gol_request_duration_seconds_sum Sum of request durations in seconds.");
        sb.AppendLine("# TYPE gol_request_duration_seconds_sum counter");
        sb.AppendLine($"gol_request_duration_seconds_sum {durationSum.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        sb.AppendLine("# HELP gol_request_duration_seconds_count Count of recorded request durations.");
        sb.AppendLine("# TYPE gol_request_duration_seconds_count counter");
        sb.AppendLine($"gol_request_duration_seconds_count {durationCount}");

        return sb.ToString();
    }
}
