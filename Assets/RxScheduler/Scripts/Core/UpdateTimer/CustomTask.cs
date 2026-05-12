using System;

/// <summary>
/// ITimerTask implementation driven by an external tick delegate.
/// The caller defines all timing logic inside the Func<float, bool> -
/// returning true keeps the task alive, false completes it
/// </summary>
public class CustomTask : ITimerTask {    
    private readonly Func<float, bool> _tick;// caller-provided delegate; receives deltaTime, returns true to continue or false to finish
        
    private bool _isActive = true;// set to false by Cancel(); guards Tick() against executing after cancellation
    public bool IsActive => _isActive;

    public CustomTask(Func<float, bool> tick) {
        _tick = tick;
    }

    public bool Tick(float deltaTime) {
        // early-out if cancelled - Tick is not invoked after Cancel()
        if(!_isActive) return false;
        return _tick(deltaTime);
    }

    public void Cancel() => _isActive = false;
}