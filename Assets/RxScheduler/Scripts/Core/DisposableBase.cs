using System;

/// <summary>
/// Base class that implements the IDisposable guard pattern.
/// Subclasses implement only OnDispose() without worrying about double-dispose
/// </summary>
public abstract class DisposableBase : IDisposable {
    private bool _disposed;

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        OnDispose();
    }

    protected abstract void OnDispose();
}