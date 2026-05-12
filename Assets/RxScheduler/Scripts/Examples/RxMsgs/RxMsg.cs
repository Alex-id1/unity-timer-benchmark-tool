using UnityEngine;

/// <summary>
/// Immutable message for UniRx MessageBroker.
/// Use factory methods to construct
/// </summary>
public class RxMsg{
    public MonoBehaviour Sender { get; }
    public RxMsgType MsgType { get; }
    public float Float { get; }
    public string Text { get; }
    public object Data { get; }
    public bool State { get; }

    //======================= Constructors and Factory Methods =======================
    private RxMsg(RxMsgType msgType, MonoBehaviour sender = null, float floatData = 0f, string text = "", object data = null, bool state = false) {
        MsgType = msgType;
        Sender = sender;
        Float = floatData;
        Text = text;
        Data = data;
        State = state;
    }

    public static RxMsg Create(RxMsgType msgType) => new(msgType);

    public static RxMsg Create(RxMsgType msgType, bool state) => new(msgType, state: state);

    public static RxMsg Create(RxMsgType msgType, float floatData) => new(msgType, floatData: floatData);

    public static RxMsg Create(RxMsgType msgType, object data) => new(msgType, data: data);

    public static RxMsg Create(RxMsgType msgType, string text, object data) => new(msgType, text: text, data: data);

    public static RxMsg Create(MonoBehaviour sender, RxMsgType msgType) => new(msgType, sender);

    public static RxMsg Create(MonoBehaviour sender, RxMsgType msgType, object data) => new(msgType, sender, data: data);
    //================================================================================
}