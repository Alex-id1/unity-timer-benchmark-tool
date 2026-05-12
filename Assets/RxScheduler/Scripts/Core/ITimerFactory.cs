/// <summary>
/// Resolves an ITimer instance by driver type.
/// Decouples timer selection logic from the presenter
/// </summary>
public interface ITimerFactory {
    ITimer Get(TimerDriver driver);
}