using UnityEngine;

/// <summary>
/// Registers the application as DPI-aware on Windows.
/// Prevents OS from upscaling the window, which causes blurry UI.
/// Attach to any GameObject in the first scene.
/// </summary>
public class DpiAwareness : MonoBehaviour {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    private void Awake() {
        SetProcessDPIAware();
    }
#endif
}