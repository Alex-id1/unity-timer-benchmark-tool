using System;

/// <summary>
/// Contract for benchmark runners. Provides lifecycle control- IsRunning for state polling,
/// Stop() for early cancellation, and Dispose() for full cleanup on scene teardown
/// </summary>
public interface IBenchmarkRunner : IDisposable {
    bool IsRunning { get; }
    void Stop();
}