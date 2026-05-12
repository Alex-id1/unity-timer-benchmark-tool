using System;

/// <summary>
/// ITimerTask that invokes a callback repeatedly at a fixed interval.
/// Runs indefinitely until cancelled - Tick() always returns true
/// </summary>
public class IntervalTask: ITimerTask {
    private float _time; // Time since last callback invocation
    private readonly float _interval; // Period between callbacks, in seconds
    private readonly Action _onTick;// Callback is invoked each time the accumulated time crosses the interval

    private bool _isActive = true;
    public bool IsActive => _isActive;

    public IntervalTask(float interval, Action onTick) {
        _interval = interval;
        _onTick = onTick;
    }

    public bool Tick(float deltaTime) {
        _time += deltaTime;

        if(_time >= _interval) {
            _time -= _interval;
            _onTick?.Invoke();
        }

        return true;
    }

    public void Cancel() => _isActive = false;
}