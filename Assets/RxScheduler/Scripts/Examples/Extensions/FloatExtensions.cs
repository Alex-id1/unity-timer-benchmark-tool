using UnityEngine;

public static class FloatExtensions {
    /// <summary>Clamps elapsed/duration to [0, 1]</summary>
    public static float NormalizedProgress(this float elapsed, float duration) => Mathf.Clamp01(elapsed / duration);
}