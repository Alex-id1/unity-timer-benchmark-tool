using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Captures a RectTransform region from the screen and saves it as PNG.
/// Reuses RenderTextures across captures. Call Dispose to release GPU memory
/// </summary>
public sealed class ScreenshotCutter : DisposableBase {
    private readonly Camera _targetCamera;
    private readonly RectTransform _targetUI;
    private readonly Canvas _canvas;
    private readonly RenderTexture _screenRTexture;
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private RenderTexture _croppedRTexture;
    private Texture2D _outputTex;

    public ScreenshotCutter(RectTransform scrShotRect) {
        _targetUI = scrShotRect;
        _canvas = _targetUI.GetComponentInParent<Canvas>();
        _targetCamera = _canvas.worldCamera;

        _screenWidth = (int)_targetCamera.pixelRect.width;
        _screenHeight = (int)_targetCamera.pixelRect.height;

        _screenRTexture = new RenderTexture(_screenWidth, _screenHeight, 24);
    }

    public IEnumerator CaptureAndSave(string path) {
        yield return new WaitForEndOfFrame();
        RectInt rect = GetClampedRect();

        CaptureScreen();
        UpdateCropRT(rect);
        CopyRegion(rect);
        ReadToTexture();

        byte[] bytes = _outputTex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }

    private void CaptureScreen() {
        _targetCamera.targetTexture = _screenRTexture;
        _targetCamera.Render();
        _targetCamera.targetTexture = null;
    }

    /// <summary>
    /// Recreates the crop RenderTexture and output Texture2D only if the rect size changed.
    /// This avoids redundant GPU allocations on repeated captures of the same region
    /// </summary>
    private void UpdateCropRT(RectInt rect) {
        if (_croppedRTexture == null || _croppedRTexture.width != rect.width || _croppedRTexture.height != rect.height) {
            if (_croppedRTexture != null)
                _croppedRTexture.Release();

            _croppedRTexture = new RenderTexture(rect.width, rect.height, 0);
            _outputTex = new Texture2D(rect.width, rect.height, TextureFormat.RGB24, false);
        }
    }

    private void CopyRegion(RectInt rect) {
        Graphics.CopyTexture(
            _screenRTexture, 0, 0,
            rect.x, rect.y, rect.width, rect.height,
            _croppedRTexture, 0, 0,
            0, 0
        );
    }

    private void ReadToTexture() {
        RenderTexture.active = _croppedRTexture;
        _outputTex.ReadPixels(new Rect(0, 0, _croppedRTexture.width, _croppedRTexture.height), 0, 0);
        _outputTex.Apply();
        RenderTexture.active = null;
    }

    /// <summary>
    /// Converts the RectTransform world corners to screen-space coordinates.
    /// Passes null camera for Screen Space - Overlay canvas, as required by WorldToScreenPoint
    /// </summary>
    private Rect GetUIRect() {
        var cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _targetCamera;

        Vector3[] corners = new Vector3[4];
        _targetUI.GetWorldCorners(corners);

        Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        return new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
    }

    /// <summary>
    /// Clamps the UI rect to screen bounds to prevent out-of-range reads in CopyRegion
    /// </summary>
    private RectInt GetClampedRect() {
        Rect r = GetUIRect();

        int x = Mathf.Clamp(Mathf.FloorToInt(r.x), 0, _screenWidth - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(r.y), 0, _screenHeight - 1);
        int width = Mathf.Clamp(Mathf.FloorToInt(r.width), 1, _screenWidth - x);
        int height = Mathf.Clamp(Mathf.FloorToInt(r.height), 1, _screenHeight - y);

        return new RectInt(x, y, width, height);
    }

    protected override void OnDispose() {
        if (_screenRTexture != null) {
            _screenRTexture.Release();
            Object.Destroy(_screenRTexture);
        }

        if (_croppedRTexture != null) {
            _croppedRTexture.Release();
            Object.Destroy(_croppedRTexture);
        }

        if (_outputTex != null) {
            Object.Destroy(_outputTex);
        }
    }
}