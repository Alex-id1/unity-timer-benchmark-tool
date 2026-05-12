using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Executes the full benchmark matrix sequentially, aggregating results across repeated runs.
/// Stateless between runs - receives all services via BenchmarkContext.
/// Caches FPS from BenchmarkPresenter.OnFpsUpdated for metric reporting.
/// Does not save results - invokes OnCompleted and exposes LastResult for the presenter to handle
/// </summary>
public class SuiteBenchmarkRunner : BenchmarkRunnerBase {
    public static event Action OnCompleted;
    public static event Action<int, int> OnProgressChanged;

    public override bool IsRunning { get; protected set; }

    public List<AggregatedResult> LastResult { get; private set; }

    private readonly Action<float> _onFpsUpdated;

    private const float MetricsTickInterval = 0.5f;
    private const float WarmupSeconds = 3f;
    private const float CooldownBetweenRuns = 1f;
    private const float CooldownBetweenScenarios = 2f;

    public SuiteBenchmarkRunner() {
        _onFpsUpdated = fps => _curFps = fps;
        BenchmarkPresenter.OnFpsUpdated += _onFpsUpdated;
    }

    // -----------------------------------------
    //  Public API
    // -----------------------------------------
    public void Run(BenchmarkContext ctx) {
        _ctx = ctx;
        IsRunning = true;

        ctx.Runner.StartCoroutine(RunSuiteCoroutine(SuiteScenarioLibrary.GetFullMatrix()));
    }

    public override void Stop() {
        IsRunning = false;
        DisposeActiveTimers();
    }

    // -----------------------------------------
    //  Execution
    // -----------------------------------------
    // Iterates the full scenario matrix sequentially.
    // Each scenario runs "repeats" times with GC.Collect() before each run
    // and cooldown pauses between runs and scenarios to reduce measurement noise
    private IEnumerator RunSuiteCoroutine(IReadOnlyList<(BenchmarkConfig config, int repeats)> scenarios) {
        yield return new WaitForSeconds(WarmupSeconds);

        var allResults = new List<AggregatedResult>(scenarios.Count);

        for (int s = 0; s < scenarios.Count; s++) {
            var (config, repeats) = scenarios[s];
            OnProgressChanged?.Invoke(s + 1, scenarios.Count);

            var runs = new List<BenchmarkResult>(repeats);

            for (int r = 0; r < repeats; r++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                yield return null;

                yield return RunSingleBenchmark(config, runs);

                yield return new WaitForSeconds(CooldownBetweenRuns);
            }

            allResults.Add(new AggregatedResult(config, runs));
            yield return new WaitForSeconds(CooldownBetweenScenarios);
        }

        LastResult = allResults;

        Stop();
        OnCompleted?.Invoke();
    }

    // Executes one benchmark run for the given config - spawns timers,
    // ticks metrics every MetricsTickInterval until duration elapses,
    // then appends the result to <paramref name="runs"/>
    private IEnumerator RunSingleBenchmark(BenchmarkConfig config, List<BenchmarkResult> runs) {
        ITimer timer = _ctx.TimerFactory.Get(config.TimerDriver);

        _ctx.Metrics.Begin(config);
        SpawnTimers(config, timer);

        int numIntervals = Mathf.Max(1, (int)(config.Duration / MetricsTickInterval));
        bool done = false;

        IDisposable metricsDisp = timer.Interval(
            MetricsTickInterval,
            _ => _ctx.Metrics.Tick(_curFps),
            numIntervals,
            () => done = true
        );

        while (!done)
            yield return null;

        metricsDisp?.Dispose();
        DisposeActiveTimers();

        runs.Add(_ctx.Metrics.Complete());
    }

    // -----------------------------------------
    //  Lifecycle
    // -----------------------------------------
    public override void Dispose() {
        BenchmarkPresenter.OnFpsUpdated -= _onFpsUpdated;
        base.Dispose();
    }
}