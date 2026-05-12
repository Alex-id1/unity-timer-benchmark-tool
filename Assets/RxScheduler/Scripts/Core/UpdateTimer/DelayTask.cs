using System;

/// <summary>
/// ITimerTask that invokes a callback once after a specified delay.
/// Tick() returns false immediately after the callback is invoked
/// </summary>
public class DelayTask: ITimerTask {
    private float _time; // time since the task started
    private readonly float _delay; // duration to wait before invoking the callback, in seconds
    private readonly Action _onComplete; // callback invoked once when the delay elapses

    private bool _isActive = true;
    public bool IsActive => _isActive;

    public DelayTask(float delay, Action onComplete) {
        _delay = delay;
        _onComplete = onComplete;
    }

    public bool Tick(float deltaTime) {
        _time += deltaTime;

        if(_time >= _delay) {
            _onComplete?.Invoke();
            _isActive = false;
            return false;
        }

        return true;
    }

    public void Cancel() => _isActive = false;
}