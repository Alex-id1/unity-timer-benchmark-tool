/// <summary>
/// Identifies the underlying timer implementation used in a benchmark run
/// </summary>
public enum TimerDriver {
    RX,
    UPDATE,
    COROUTINE
}