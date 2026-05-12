/// <summary>
/// Message types published via MessageBroker for fire-and-forget cross-component events.
/// Preferred over direct event subscriptions where components have no reason to know about each other
/// </summary>
public enum RxMsgType {
    CREATE_CHART,
    DO_SCREEN_BLUR_IN,
    DO_SCREEN_BLUR_OUT,
    SHOW_POPUP,
    HIDE_POPUP
}