using System;
using System.Collections.Generic;

/// <summary>
/// Shared execution logic for all benchmark runners.
/// Holds only transient run-state (active timers, cached FPS for metrics).
/// All shared services are provided via BenchmarkContext on each Run() call.
/// FPS calculation lives in BenchmarkPresenter. Runners receive updates via OnFpsUpdated
/// </summary>
public abstract class BenchmarkRunnerBase : IBenchmarkRunner {
    protected BenchmarkContext _ctx;
    protected ITimer _curTimer;
    protected float _curFps;

    protected readonly List<IDisposable> _activeTimers = new();

    public abstract bool IsRunning { get; protected set; }
    public abstract void Stop();

    // -----------------------------------------
    //  Timer spawning
    // -----------------------------------------
    protected void SpawnTimers(BenchmarkConfig config, ITimer timer) {
        for (int i = 0; i < config.NumOfInstances; i++) {
            IDisposable d = config.TimerType switch {
                TimerType.DELAY => timer.Delay(config.Duration, null),
                TimerType.TIMER => timer.Timer(config.Duration, null, null),
                TimerType.INTERVAL => timer.Interval(config.Duration, null, config.NumOfIntervals, null),
                TimerType.COUNTDOWN => timer.Countdown(config.Duration, null, null),
                _ => throw new ArgumentOutOfRangeException()
            };
            _activeTimers.Add(d);
        }
    }

    protected void DisposeActiveTimers() {
        foreach (var d in _activeTimers) d?.Dispose();
        _activeTimers.Clear();
    }

    // -----------------------------------------
    //  Lifecycle
    // -----------------------------------------
    public virtual void Dispose() => DisposeActiveTimers();
}