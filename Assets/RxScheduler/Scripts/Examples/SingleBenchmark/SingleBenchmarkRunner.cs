using System;
using System.Collections;

/// <summary>
/// Executes a single benchmark run driven by a BenchmarkConfig.
/// Stateless between runs - receives all services via BenchmarkContext.
/// Caches FPS from BenchmarkPresenter.OnFpsUpdated for metric reporting
/// </summary>
public class SingleBenchmarkRunner : BenchmarkRunnerBase {
    public static event Action<BenchmarkResult> OnCompleted;

    public override bool IsRunning { get; protected set; }

    public BenchmarkResult LastResult { get; private set; }

    private IDisposable  _benchmarkIntervalDisp;
    private Action<float> _onFpsUpdated;

    public SingleBenchmarkRunner() {
        _onFpsUpdated = fps => _curFps = fps;
        BenchmarkPresenter.OnFpsUpdated += _onFpsUpdated;
    }

    // -----------------------------------------
    //  Public API
    // -----------------------------------------
    public void Run(BenchmarkContext ctx, BenchmarkConfig config) {
        _ctx      = ctx;
        IsRunning = true;
        ctx.Runner.StartCoroutine(RunCoroutine(config));
    }

    /// <summary>
    /// Stops all active timers, finalizes metrics and fires OnCompleted.
    /// Called both on natural completion and on manual cancellation from the presenter
    /// </summary>
    public override void Stop() {
        if (!IsRunning) return;
        IsRunning = false;

        _benchmarkIntervalDisp?.Dispose();
        DisposeActiveTimers();

        LastResult = _ctx.Metrics.Complete();
        OnCompleted?.Invoke(LastResult);
    }

    // -----------------------------------------
    //  Execution
    // -----------------------------------------
    /// <summary>
    /// Forces GC collection before the run to reduce noise, then spawns timers and samples metrics every 0.5s for the configured duration.
    /// Calls Stop() on the final interval tick to complete the run
    /// </summary>
    private IEnumerator RunCoroutine(BenchmarkConfig config) {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        yield return null;

        _ctx.Metrics.Begin(config);
        _curTimer = _ctx.TimerFactory.Get(config.TimerDriver);

        SpawnTimers(config, _curTimer);

        const float intervalSeconds = 0.5f;
        int numOfIntervals = (int)(config.Duration / intervalSeconds);

        _benchmarkIntervalDisp = _curTimer.Interval(
            intervalSeconds,
            _ => _ctx.Metrics.Tick(_curFps),
            numOfIntervals,
            () => Stop()
        );
    }

    // -----------------------------------------
    //  Lifecycle
    // -----------------------------------------
    public override void Dispose() {
        BenchmarkPresenter.OnFpsUpdated -= _onFpsUpdated;
        base.Dispose();
        _benchmarkIntervalDisp?.Dispose();
    }
}