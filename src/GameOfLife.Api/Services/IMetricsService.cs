namespace GameOfLife.Api.Services;

public interface IMetricsService
{
    void IncrementRequest();
    void IncrementException();
    void ObserveRequestDuration(double seconds);
    string GetMetricsText();
}
