using UnityEngine;

/// <summary>
/// Ties a running Unity coroutine to the IDisposable contract.
/// Stopping the coroutine on Dispose - via the same MonoBehaviour that started it
/// </summary>
public sealed class CoroutineDisposable : DisposableBase {
    private readonly MonoBehaviour _runner;//MonoBehaviour that owns and can stop the coroutine
    private Coroutine _coroutine; //handle returned by StartCoroutine; nulled after stop to prevent double-stop events

    public CoroutineDisposable(MonoBehaviour runner, Coroutine coroutine) {
        _runner = runner;
        _coroutine = coroutine;
    }

    /// <summary>
    /// null-check _runner guards against the MonoBehaviour being destroyed before Dispose is called
    /// </summary>
    protected override void OnDispose() {
        if (_coroutine != null && _runner != null) {
            _runner.StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }
}