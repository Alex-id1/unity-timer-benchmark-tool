using System;
using UnityEngine;
using UniRx;

/// <summary>
/// ITimer implementation based on UniRx.
/// All methods return IDisposable - dispose to cancel early
/// </summary>
public class RxTimer : TimerBase {

    // -----------------------------------------
    //  DELAY
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnDelay(float delaySeconds, Action onComplete) {
        return Observable.Timer(TimeSpan.FromSeconds(delaySeconds))
            .Subscribe(_ => onComplete?.Invoke());
    }

    // -----------------------------------------
    //  INTERVAL
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnInterval(float intervalSeconds, Action onTick) {
        return Observable.Interval(TimeSpan.FromSeconds(intervalSeconds))
            .Subscribe(_ => onTick?.Invoke());
    }

    /// <inheritdoc/>
    protected override IDisposable OnIntervalCount(float intervalSeconds, Action<int> onTick, int totalTicks, Action onComplete) {
        return Observable.Interval(TimeSpan.FromSeconds(intervalSeconds))
            .Take(totalTicks)
            .Subscribe(
                i => onTick?.Invoke((int)i + 1),
                () => onComplete?.Invoke()
            );
    }

    // -----------------------------------------
    //  TIMER (progress: 0..1)
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnTimer(float durationSeconds, Action<float> onProgress, Action onComplete) {
        float elapsed = 0f;

        return Observable.EveryUpdate()
            .TakeWhile(_ => elapsed < durationSeconds)
            .Subscribe(
                _ => {
                    elapsed += Time.deltaTime;
                    onProgress?.Invoke(elapsed.NormalizedProgress(durationSeconds));
                },
                () => {
                    onProgress?.Invoke(1f); // guarantee final progress = 1
                    onComplete?.Invoke();
                }
            );
    }

    // -----------------------------------------
    //  COUNTDOWN (remaining n..0)
    // -----------------------------------------
    /// <inheritdoc/>
    protected override IDisposable OnCountdown(float durationSeconds, Action<float> onRemaining, Action onComplete) {
        float remaining = durationSeconds;

        return Observable.EveryUpdate()
            .TakeWhile(_ => remaining > 0f)
            .Subscribe(
                _ => {
                    remaining -= Time.deltaTime;
                    onRemaining?.Invoke(Mathf.Max(0f, remaining));
                },
                () => {
                    onRemaining?.Invoke(0f); // guarantee final remaining = 0
                    onComplete?.Invoke();
                }
            );
    }
}