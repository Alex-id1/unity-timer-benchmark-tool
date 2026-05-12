using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Animates a sprite-sheet loader while a benchmark is running.
/// Cycles through sprites to animate the tail fade, then rotates the image
/// by one segment - producing a smooth spinning effect.
/// Shows on DO_SCREEN_BLUR_IN, hides on DO_SCREEN_BLUR_OUT Rx messages
/// </summary>
public class LoaderView : MonoBehaviour{

    [SerializeField] private Image _loaderImage;
    [SerializeField] private Sprite[] _loaderSprites;
    [SerializeField] private float _rotationDuration = 3.5f;


    private const int SegmentsCount = 12;
    private const string NoImageComponentError = "[LoaderImg] Loader image not found. Attach the loader image component to this script";

    private Color _opaqueColor, _transparentColor;
    private ITimer _curTimer;
    private IDisposable _disposable;



    // -----------------------------------------
    //  Unity
    // -----------------------------------------
    private void Awake() {
        if(_loaderImage == null) {
            Debug.LogError(NoImageComponentError);
            return;
        }
        _opaqueColor = _loaderImage.color;
        _transparentColor = new Color(_opaqueColor.r, _opaqueColor.g, _opaqueColor.b, 0f);

        _loaderImage.color = _transparentColor;
        _loaderImage.enabled = false;

        MessageBroker.Default.Receive<RxMsg>().Subscribe(OnMessage).AddTo(this);
    }

    private void OnDestroy() {
        StopLoader();
    }

    // -----------------------------------------
    //  Message handling
    // -----------------------------------------
    private void OnMessage(RxMsg msg) {
        switch(msg.MsgType) {
            case RxMsgType.DO_SCREEN_BLUR_IN: StartLoader(msg.Data); break;
            case RxMsgType.DO_SCREEN_BLUR_OUT: StopLoader(); break;
        }
    }

    // -----------------------------------------
    //  Loader control
    // -----------------------------------------
    /// <summary>
    /// Waits for the blur to settle, then fades in the loader and starts
    /// the interval timer that controls TickRoutine
    /// </summary>
    private void StartLoader(object data) {
        ITimer timer = data as ITimer;
        if(timer == null)
            return;

        if(_loaderSprites.Length == 0)
            return;
        _curTimer = timer;
        StartCoroutine(StartLoaderRoutine());
    }

    private IEnumerator StartLoaderRoutine() {
        yield return new WaitForSeconds(0.5f);
        float tickInterval = _rotationDuration / SegmentsCount;
        float rotationAngle = 360f / SegmentsCount;
        float spriteInterval = tickInterval / _loaderSprites.Length;

        _loaderImage.color = _transparentColor;
        _loaderImage.enabled = true;
        _loaderImage.transform.localEulerAngles = Vector3.zero;
        _loaderImage.DOFade(1f, 1f);

        _disposable = _curTimer?.Interval(
            tickInterval,
            () => StartCoroutine(TickRoutine(rotationAngle, spriteInterval)));
    }

    /// <summary>
    /// Sequentially cycles through all sprites at an interval matching the animation step to simulate tail fading,
    /// then snaps the image rotation by one segment
    /// </summary>
    private IEnumerator TickRoutine(float rotationAngle, float spriteInterval) {
        foreach(var sprite in _loaderSprites) {
            _loaderImage.sprite = sprite;
            yield return new WaitForSeconds(spriteInterval);
        }

        Vector3 current = _loaderImage.transform.localEulerAngles;
        current.z -= rotationAngle;
        _loaderImage.transform.localEulerAngles = current;
    }


    private void StopLoader() {
        _disposable?.Dispose();
        _disposable = null;

        if(_loaderImage == null)
            return;

        _loaderImage.DOFade(0f, 0.3f).OnComplete(() => {
            _loaderImage.transform.localEulerAngles = Vector3.zero;
            _loaderImage.enabled = false;
        });
    }
}