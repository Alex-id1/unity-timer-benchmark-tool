using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Repositions itself beside the trigger icon each time Show is called
/// </summary>
public class InfoTooltipView : MonoBehaviour {
    [SerializeField] private CanvasGroup _toltipCanvasGroup_Single;
    [SerializeField] private CanvasGroup _toltipCanvasGroup_Suite;

    [Tooltip("Pixel offset from the trigger icon in canvas local space")]
    [SerializeField] private Vector2 _offset = new Vector2(12f, -12f);

    private const float FadeInDuration = 0.15f;
    private const float FadeOutDuration = 0.1f;

    private RectTransform _rt_Single, _rt_Suite;
    private Camera _canvasCamera;

    private void Awake() {
        _rt_Single = _toltipCanvasGroup_Single.GetComponent<RectTransform>();
        _rt_Suite = _toltipCanvasGroup_Suite.GetComponent<RectTransform>();

        var rootCanvas = _toltipCanvasGroup_Single.GetComponentInParent<Canvas>().rootCanvas;
        _canvasCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

        _toltipCanvasGroup_Single.alpha = 0f;
        _toltipCanvasGroup_Suite.alpha = 0f;

        SetTooltipsOff();
    }

    private void OnEnable() {
        InfoTrigger.OnHoverEnter += Show;
        InfoTrigger.OnHoverExit += Hide;
    }
    private void OnDisable() {
        InfoTrigger.OnHoverEnter -= Show;
        InfoTrigger.OnHoverExit -= Hide;
    }

    private void Show(BenchmarkType benchmarkType, PointerEventData pointerData) {
        var toolTipRT = GetToltipRectTransform(benchmarkType);

        if(toolTipRT == null) return;

        Vector2 screenPoint = pointerData.position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(toolTipRT.parent as RectTransform, screenPoint, _canvasCamera, out Vector2 localPoint);

        toolTipRT.anchoredPosition = localPoint + _offset;

        if(!toolTipRT.gameObject.activeSelf) toolTipRT.gameObject.SetActive(true);
        
        var canvGroup = GetToltipCanvasGroup(benchmarkType);
        canvGroup.DOKill();
        canvGroup.DOFade(1f, FadeInDuration);
    }

    private void Hide(BenchmarkType benchmarkType) {
        var canvGroup = GetToltipCanvasGroup(benchmarkType);
        canvGroup.DOKill();
        canvGroup.DOFade(0f, FadeOutDuration).OnComplete(() => SetTooltipsOff());
    }

    private void SetTooltipsOff() {
        if(_rt_Single.gameObject.activeSelf) _rt_Single.gameObject.SetActive(false);
        if(_rt_Suite.gameObject.activeSelf)_rt_Suite.gameObject.SetActive(false);
    }

    private RectTransform GetToltipRectTransform(BenchmarkType benchmarkType) => benchmarkType switch {
        BenchmarkType.SINGLE => _rt_Single,
        BenchmarkType.SUITE => _rt_Suite,
        _ => throw new ArgumentOutOfRangeException(nameof(benchmarkType))
    };
    private CanvasGroup GetToltipCanvasGroup(BenchmarkType benchmarkType) => benchmarkType switch {
        BenchmarkType.SINGLE => _toltipCanvasGroup_Single,
        BenchmarkType.SUITE => _toltipCanvasGroup_Suite,
        _ => throw new ArgumentOutOfRangeException(nameof(benchmarkType))
    };
}