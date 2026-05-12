
/// <summary>
/// Contract for a single timer task managed by UpdateTimerRunner.
/// Tick() is called every frame with deltaTime - returns false when the task is done
/// </summary>
public interface ITimerTask {
    bool IsActive { get; }
    bool Tick(float deltaTime); // false = finished
    void Cancel();
}