/// <summary>
/// Identifies the validation error shown in a popup, used to highlight
/// the corresponding UI control that caused it.
/// Provided by BenchmarkPresenter to BenchmarkView via ShowPopupWithHighlight()
/// </summary>
public enum PopupMsgType{
    DEFAULT,
    ERROR_NO_METRICS_SELECTED,
    ERROR_ZERO_TIMER_INSTANCES,
    ERROR_ZERO_DURATION,
    ERROR_ZERO_INTERVAL_NUMBER,
}