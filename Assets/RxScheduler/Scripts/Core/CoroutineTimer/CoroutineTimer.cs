using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ITimer implementation based on Coroutine.
/// All methods return IDisposable - dispose to cancel early
/// </summary>
public class CoroutineTimer : TimerBase {
    private readonly MonoBehaviour _runner;

    public CoroutineTimer(MonoBehaviour runner) {
        _runner = runner;
    }

    // -----------------------------------------
    //  DELAY
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnDelay(float delaySeconds, Action onComplete) {
        var coroutine = _runner.StartCoroutine(DelayRoutine(delaySeconds, onComplete));
        return new CoroutineDisposable(_runner, coroutine);
    }

    private IEnumerator DelayRoutine(float delay, Action onComplete) {
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    // -----------------------------------------
    //  INTERVAL
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnInterval(float intervalSeconds, Action onTick) {
        var coroutine = _runner.StartCoroutine(IntervalRoutine(intervalSeconds, onTick));
        return new CoroutineDisposable(_runner, coroutine);
    }

    private IEnumerator IntervalRoutine(float interval, Action onTick) {
        while (true) {
            yield return new WaitForSeconds(interval);
            onTick?.Invoke();
        }
    }

    /// <inheritdoc/>
    protected override IDisposable OnIntervalCount(float intervalSeconds, Action<int> onTick, int totalTicks, Action onComplete) {
        var coroutine = _runner.StartCoroutine(IntervalCountRoutine(intervalSeconds, onTick, totalTicks, onComplete));
        return new CoroutineDisposable(_runner, coroutine);
    }

    private IEnumerator IntervalCountRoutine(float interval, Action<int> onTick, int count, Action onComplete) {
        for (int i = 1; i <= count; i++) {
            yield return new WaitForSeconds(interval);
            onTick?.Invoke(i);
        }
        onComplete?.Invoke();
    }

    // -----------------------------------------
    //  TIMER (progress 0..1)
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnTimer(float durationSeconds, Action<float> onProgress, Action onComplete) {
        var coroutine = _runner.StartCoroutine(TimerRoutine(durationSeconds, onProgress, onComplete));
        return new CoroutineDisposable(_runner, coroutine);
    }

    private IEnumerator TimerRoutine(float duration, Action<float> onProgress, Action onComplete) {
        float time = 0f;

        while (time < duration) {
            time += Time.deltaTime;
            onProgress?.Invoke(time.NormalizedProgress(duration));
            yield return null;
        }

        onProgress?.Invoke(1f); // guarantee final progress = 1
        onComplete?.Invoke();
    }

    // -----------------------------------------
    //  COUNTDOWN (remaining n..0)
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnCountdown(float durationSeconds, Action<float> onRemaining, Action onComplete) {
        var coroutine = _runner.StartCoroutine(CountdownRoutine(durationSeconds, onRemaining, onComplete));
        return new CoroutineDisposable(_runner, coroutine);
    }

    private IEnumerator CountdownRoutine(float duration, Action<float> onRemaining, Action onComplete) {
        float time = duration;

        while (time > 0f) {
            onRemaining?.Invoke(Mathf.Max(0f, time));
            time -= Time.deltaTime;
            yield return null;
        }

        onRemaining?.Invoke(0f); // guarantee final remaining = 0
        onComplete?.Invoke();
    }
}