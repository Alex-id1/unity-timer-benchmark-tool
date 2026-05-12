/// <summary>
/// Execution context passed to benchmark runners on each Run() call.
/// Owned exclusively by BenchmarkPresenter. Runners borrow it and never own it.
/// </summary>
public class BenchmarkContext {
    public readonly ITimerFactory TimerFactory;
    public readonly IMetricsCollector Metrics;
    public readonly UpdateTimerRunner Runner;

    public BenchmarkContext(ITimerFactory timerFactory, IMetricsCollector metrics, UpdateTimerRunner runner) {
        TimerFactory = timerFactory;
        Metrics = metrics;
        Runner = runner;
    }
}