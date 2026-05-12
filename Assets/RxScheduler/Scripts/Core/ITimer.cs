using System;

public interface ITimer {

    // -----------------------------------------
    //  DELAY - once after N seconds
    // -----------------------------------------
    /// <summary>
    /// Calls onComplete once after delaySeconds
    /// </summary>
    IDisposable Delay(float delaySeconds, Action onComplete);


    // -----------------------------------------
    //  INTERVAL - repeat every N seconds
    // -----------------------------------------

    /// <summary>
    /// Calls onTick every intervalSeconds indefinitely
    /// </summary>
    IDisposable Interval(float intervalSeconds, Action onTick);

    /// <summary>
    /// Calls onTick every intervalSeconds totalTicks number of times, then onComplete
    /// </summary>
    IDisposable Interval(float intervalSeconds, Action<int> onTick, int totalTicks, Action onComplete = null );

    // -----------------------------------------
    //  TIMER
    // -----------------------------------------

    /// <summary>
    /// Calls onTick Action every seconds, and finally onComplete
    /// </summary>
    IDisposable Timer(float durationSeconds, Action<float> onProgress, Action onComplete = null);

    // -----------------------------------------
    //  COUNTDOWN
    // -----------------------------------------

    /// <summary>
    /// Countdown from durationSeconds to 0
    /// onTick gets the remaining time (float, in seconds)
    /// onComplete is called when it reaches 0
    /// </summary>
    IDisposable Countdown(float durationSeconds, Action<float> onRemaining, Action onComplete = null);
}