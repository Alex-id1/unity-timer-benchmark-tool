using System;

/// <summary>
/// ITimerTask that invokes a callback at a fixed interval for a set number of ticks,
/// then invokes a completion callback and stops. Tick() returns false when done
/// </summary>
public class IntervalCountTask: ITimerTask {
    private float _time; // time since last tick
    private int _current; // total number of ticks executed so far. Passed to the onTick callback
    private readonly float _interval; // period between ticks, in seconds
    private readonly int _totalTicks; // total number of ticks before completion
    private readonly Action<int> _onTick; // callback invoked for each tick. Receives the current tick count
    private readonly Action _onComplete; // callback invoked once when the last tick is reached
    private bool _isActive = true;

    public bool IsActive => _isActive;

    public IntervalCountTask(float interval, int totalTicks, Action<int> onTick, Action onComplete) {
        _interval = interval;
        _totalTicks = totalTicks;
        _onTick = onTick;
        _onComplete = onComplete;
    }

    public bool Tick(float deltaTime) {
        _time += deltaTime;

        if(_time >= _interval) {
            _time -= _interval;
            _current++;
            _onTick?.Invoke(_current);

            if(_current >= _totalTicks) {
                _onComplete?.Invoke();
                _isActive = false;
                return false;
            }
        }

        return true;
    }

    public void Cancel() => _isActive = false;
}