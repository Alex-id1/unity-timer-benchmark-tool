using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to an "i" icon. Fires static events on hover - knows nothing about the tooltip view.
/// InfoTooltipPresenter subscribes and drives InfoTooltipView
/// </summary>
public class InfoTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private BenchmarkType benchmarkType = BenchmarkType.SINGLE;
    public static event Action<BenchmarkType, PointerEventData> OnHoverEnter;
    public static event Action<BenchmarkType> OnHoverExit;

    public void OnPointerEnter(PointerEventData eventData) => OnHoverEnter?.Invoke(benchmarkType, eventData);
    public void OnPointerExit(PointerEventData eventData)  => OnHoverExit?.Invoke(benchmarkType);
}