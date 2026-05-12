using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour that drives all Update-based timer tasks.
/// Maintains an active task list and a pending buffer. Tasks added during
/// Update are staged in _pending and merged at the start of the next frame
/// to avoid modifying the active list during iteration.
/// </summary>
public class UpdateTimerRunner: MonoBehaviour {
    private readonly List<ITimerTask> _tasks = new(); // active tasks ticked every frame. Iterated back-to-front for safe tasks removal
    private readonly List<ITimerTask> _pending = new(); // tasks registered during the current frame. Merged into _tasks on next Update
    public bool IsRunning => _tasks.Count > 0 || _pending.Count > 0;

    /// <summary>
    /// Stage in _pending to avoid modifying _tasks during active iteration
    /// </summary>
    /// <returns>TaskDisposable</returns>
    public IDisposable AddTask(ITimerTask task) {
        _pending.Add(task);
        return new TaskDisposable(task);
    }

    // Merge pending first, then tick - back-to-front loop allows RemoveAt without index shift
    private void Update() {
        if(_pending.Count > 0) {
            _tasks.AddRange(_pending);
            _pending.Clear();
        }

        float delta = Time.deltaTime;

        for(int i = _tasks.Count - 1; i >= 0; i--) {
            ITimerTask task = _tasks[i];
            if(!task.IsActive || !task.Tick(delta)) {
                //print($"taskList count: {taskList.Count}; RemoveAt: {i}");
                _tasks.RemoveAt(i);
            }
        }
    }
}