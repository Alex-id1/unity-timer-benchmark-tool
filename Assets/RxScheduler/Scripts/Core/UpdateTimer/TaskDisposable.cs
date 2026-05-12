/// <summary>
/// Wraps an ITimerTask in the IDisposable contract.
/// Cancels the task on Dispose, allowing update-based timers
/// to participate in the standard using / AddTo(this) lifecycle
/// </summary>
public sealed class TaskDisposable : DisposableBase {
    private readonly ITimerTask _task;

    public TaskDisposable(ITimerTask task) {
        _task = task;
    }

    protected override void OnDispose() => _task.Cancel();
}