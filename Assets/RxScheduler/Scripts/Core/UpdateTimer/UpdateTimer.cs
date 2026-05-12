using System;

/// <summary>
/// ITimer implementation based on Update().
/// All methods return IDisposable - dispose to cancel early
/// </summary>
public class UpdateTimer : TimerBase {
    private readonly UpdateTimerRunner _runner;

    public UpdateTimer(UpdateTimerRunner runner) {
        _runner = runner;
    }

    // -----------------------------------------
    //  DELAY
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnDelay(float delaySeconds, Action onComplete) {
        return _runner.AddTask(new DelayTask(delaySeconds, onComplete));
    }

    // -----------------------------------------
    //  INTERVAL
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnInterval(float intervalSeconds, Action onTick) {
        return _runner.AddTask(new IntervalTask(intervalSeconds, onTick));
    }

    /// <inheritdoc/>
    protected override IDisposable OnIntervalCount(float intervalSeconds, Action<int> onTick, int totalTicks, Action onComplete) {
        return _runner.AddTask(new IntervalCountTask(intervalSeconds, totalTicks, onTick, onComplete));
    }

    // -----------------------------------------
    //  TIMER (progress: 0..1)
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnTimer(float durationSeconds, Action<float> onProgress, Action onComplete) {
        float elapsed = 0f;

        return _runner.AddTask(new CustomTask(delta => {
            elapsed += delta;

            if (elapsed >= durationSeconds) {
                onProgress?.Invoke(1f);
                onComplete?.Invoke();
                return false;
            }

            onProgress?.Invoke(elapsed.NormalizedProgress(durationSeconds));
            return true;
        }));
    }

    // -----------------------------------------
    //  COUNTDOWN (remaining n..0)
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnCountdown(float durationSeconds, Action<float> onRemaining, Action onComplete) {
        float remaining = durationSeconds;

        return _runner.AddTask(new CustomTask(delta => {
            remaining -= delta;

            if (remaining <= 0) {
                onRemaining?.Invoke(0f);
                onComplete?.Invoke();
                return false;
            }

            onRemaining?.Invoke(remaining);
            return true;
        }));
    }
}