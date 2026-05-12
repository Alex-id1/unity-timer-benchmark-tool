

/// <summary>
/// Contract for per-interval metrics sampling during a benchmark run.
/// Begin() -> Tick() x N -> Complete() - called in strict sequence by the runner
/// </summary>
public interface IMetricsCollector {
    /// <summary>Start collecting metrics for the config</summary>
    void Begin(BenchmarkConfig config);

    /// <summary>Record one sample - called every N seconds during the test</summary>
    void Tick(float curFps);

    /// <summary>Complete data collection and return the result</summary>
    BenchmarkResult Complete();
}