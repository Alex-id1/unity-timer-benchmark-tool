using System;
using System.Collections.Generic;

/// <summary>
/// Maps TimerDriver enum values to ITimer instances.
/// Adding a new driver requires only registering it in this class
/// </summary>
public sealed class TimerFactory : ITimerFactory {
    private readonly IReadOnlyDictionary<TimerDriver, ITimer> _timers;

    public TimerFactory(ITimer rxTimer, ITimer updateTimer, ITimer coroutineTimer) {
        _timers = new Dictionary<TimerDriver, ITimer> {
            { TimerDriver.RX, rxTimer },
            { TimerDriver.UPDATE, updateTimer },
            { TimerDriver.COROUTINE, coroutineTimer }
        };
    }

    public ITimer Get(TimerDriver driver) {
        if (_timers.TryGetValue(driver, out var timer))
            return timer;

        throw new ArgumentOutOfRangeException(nameof(driver), driver, "No ITimer registered for this driver");
    }
}