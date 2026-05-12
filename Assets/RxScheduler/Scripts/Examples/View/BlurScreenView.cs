using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Captures the screen, applies a box blur and displays the result as a fullscreen overlay.
/// Shader is loaded from Resources at startup. Blur is triggered by DO_SCREEN_BLUR_IN, hidden by DO_SCREEN_BLUR_OUT
/// </summary>
public class BlurScreenView : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private RawImage _targetImage;

    [Header("Quality")]
    [Range(128, 1024)][SerializeField] private int _textureSize = 512;
    [Range(1, 4)][SerializeField] private int _blurPasses = 2;
    [Range(0.5f, 3f)][SerializeField] private float _blurSize = 1.5f;

    private Canvas _canvas;
    private Camera _sourceCamera;

    private const float fadeDuration = 0.3f;

    private const string ShaderResourcePath = "FastBoxBlur";

    private Material _blurMaterial;

    // -----------------------------------------
    //  Unity
    // -----------------------------------------
    private void Awake() {
        var shader = Resources.Load<Shader>(ShaderResourcePath);

        if(shader == null) {
            Debug.LogError($"[BlurScreenView] Shader '{ShaderResourcePath}' not found in Resources");
            enabled = false;
            return;
        }
        _blurMaterial = new Material(shader);

        if(_targetImage == null) {
            Debug.LogError("[BlurScreenView] Target image is not assigned");
            return;
        }
        _canvas = _targetImage.GetComponentInParent<Canvas>();
        _sourceCamera = _canvas != null ? _canvas.worldCamera : Camera.main;

        if(_sourceCamera == null) {
            Debug.LogError("[BlurScreenView] No camera found - assign a camera to the Canvas or ensure Camera.main exists");
            enabled = false;
            return;
        }

        ResetTargetImage();

        MessageBroker.Default.Receive<RxMsg>().Subscribe(OnMessage).AddTo(this);
    }

    private void OnDestroy() {
        if(_blurMaterial != null)
            Destroy(_blurMaterial);

        ReleaseOutputTexture();
    }

    // -----------------------------------------
    //  Message handling
    // -----------------------------------------
    private void OnMessage(RxMsg msg) {
        switch(msg.MsgType) {
            case RxMsgType.DO_SCREEN_BLUR_IN: CaptureAndBlur(); break;
            case RxMsgType.DO_SCREEN_BLUR_OUT: DisableBlur(); break;
        }
    }

    // -----------------------------------------
    //  Blur
    // -----------------------------------------

    // Captures the current frame, applies multi-pass box blur via RenderTextures,
    // reads the result into a Texture2D and sets it as the overlay.
    // RenderTextures are released immediately after the read
    private void CaptureAndBlur() {
        int height = Mathf.RoundToInt(_textureSize * (float)Screen.height / Screen.width);

        RenderTexture rt = RenderTexture.GetTemporary(_textureSize, height, 0);
        RenderTexture tempRt = RenderTexture.GetTemporary(_textureSize, height, 0);

        RenderToTexture(rt);
        ApplyBlur(rt, tempRt);

        Texture2D output = ReadToTexture2D(rt);

        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.ReleaseTemporary(tempRt);

        SetOutputTexture(output);
    }
    private void DisableBlur() {
        _targetImage.DOFade(0f, fadeDuration).OnComplete(ResetTargetImage);
    }

    private void RenderToTexture(RenderTexture target) {
        _sourceCamera.targetTexture = target;
        _sourceCamera.Render();
        _sourceCamera.targetTexture = null;
    }

    private void ApplyBlur(RenderTexture rt, RenderTexture temp) {
        _blurMaterial.SetFloat("_BlurSize", _blurSize);

        for(int i = 0; i < _blurPasses; i++) {
            Graphics.Blit(rt, temp, _blurMaterial);
            Graphics.Blit(temp, rt, _blurMaterial);
        }
    }

    private Texture2D ReadToTexture2D(RenderTexture source) {
        var tex = new Texture2D(source.width, source.height, TextureFormat.RGB24, false, true);

        RenderTexture.active = source;
        tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        return tex;
    }

    // -----------------------------------------
    //  Output texture management
    // -----------------------------------------

    // Releases the previous output texture, assigns the new one and fades in the overlay
    private void SetOutputTexture(Texture2D tex) {
        ReleaseOutputTexture();

        _targetImage.texture = tex;
        _targetImage.enabled = true;
        _targetImage.color = new Color(1f, 1f, 1f, 0f);
        _targetImage.DOFade(1f, fadeDuration);
    }

    private void ReleaseOutputTexture() {
        if(_targetImage.texture != null) {
            Destroy(_targetImage.texture);
        }
    }

    private void ResetTargetImage() {
        if(_targetImage == null)
            return;
        _targetImage.enabled = false;
        _targetImage.color = new Color(1f, 1f, 1f, 0f);
    }
}