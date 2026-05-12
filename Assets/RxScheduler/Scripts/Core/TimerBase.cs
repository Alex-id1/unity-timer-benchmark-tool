using System;

/// <summary>
/// Template Method base for all ITimer implementations.
/// Parameter validation is handled in one place, so subclasses only implement the On* hooks logic
/// </summary>
public abstract class TimerBase : ITimer {

    // -----------------------------------------
    //  Public ITimer - validate then delegate
    // -----------------------------------------
    public IDisposable Delay(float delaySeconds, Action onComplete) {
        ValidateDuration(delaySeconds);
        return OnDelay(delaySeconds, onComplete);
    }

    public IDisposable Interval(float intervalSeconds, Action onTick) {
        ValidateDuration(intervalSeconds);
        return OnInterval(intervalSeconds, onTick);
    }

    public IDisposable Interval(float intervalSeconds, Action<int> onTick, int totalTicks, Action onComplete = null) {
        ValidateDuration(intervalSeconds);
        ValidateCount(totalTicks);
        return OnIntervalCount(intervalSeconds, onTick, totalTicks, onComplete);
    }

    public IDisposable Timer(float durationSeconds, Action<float> onProgress, Action onComplete = null) {
        ValidateDuration(durationSeconds);
        return OnTimer(durationSeconds, onProgress, onComplete);
    }

    public IDisposable Countdown(float durationSeconds, Action<float> onRemaining, Action onComplete = null) {
        ValidateDuration(durationSeconds);
        return OnCountdown(durationSeconds, onRemaining, onComplete);
    }

    // -----------------------------------------
    //  Hooks - implemented by subclasses
    // -----------------------------------------
    protected abstract IDisposable OnDelay(float delaySeconds, Action onComplete);
    protected abstract IDisposable OnInterval(float intervalSeconds, Action onTick);
    protected abstract IDisposable OnIntervalCount(float intervalSeconds, Action<int> onTick, int totalTicks, Action onComplete);
    protected abstract IDisposable OnTimer(float durationSeconds, Action<float> onProgress, Action onComplete);
    protected abstract IDisposable OnCountdown(float durationSeconds, Action<float> onRemaining, Action onComplete);

    // -----------------------------------------
    //  Validation
    // -----------------------------------------
    private static void ValidateDuration(float seconds) {
        if (seconds <= 0f) throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Duration must be positive");
    }

    private static void ValidateCount(int count) {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be positive");
    }
}