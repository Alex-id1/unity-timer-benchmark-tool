using DG.Tweening;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a dismissible message popup with auto-hide after a configurable delay.
/// Optionally highlights a UI element that caused the error - fades it to pink, then restores.
/// Listens to SHOW_POPUP and HIDE_POPUP messages via MessageBroker
/// </summary>
public class PopupView : MonoBehaviour {
    [SerializeField] private Text _messageText;
    [SerializeField] private float _autoDismissDelay = 3f;
    [SerializeField] private float _fadeDuration = 0.25f;

    private Sequence _msgTextSeq, _highlightOtherElmntSeq;
    private Graphic highlightOther;
    private Dictionary<int, Color> originalColorCache = new Dictionary<int, Color>();
    private Color pinkColor = new Color(1f, 0.5f, 0.5f, 1f);

    private void Awake() {
        MessageBroker.Default.Receive<RxMsg>().Subscribe(OnReceiveRxMsg).AddTo(this);
        InitGraphic(_messageText);
    }

    private void OnDestroy() {
        _msgTextSeq?.Kill();
        _highlightOtherElmntSeq?.Kill();
    }

    private void OnReceiveRxMsg(RxMsg msg) {
        switch (msg.MsgType) {
            case RxMsgType.SHOW_POPUP: ShowPopup(msg); break;
            case RxMsgType.HIDE_POPUP: HidePopup(); break;
        }
    }

    /// <summary>
    /// Shows the message text with a fade-in/auto-closing sequence.
    /// If the message contains a Graphic target, highlights it in pink for the duration of its displaying
    /// </summary>
    private void ShowPopup(RxMsg msg) {
        _messageText.gameObject.SetActive(true);
        _messageText.text = msg.Text;

        if (_msgTextSeq == null) {
            _msgTextSeq = DOTween.Sequence();
            _msgTextSeq.Append(_messageText.DOFade(1f, 0.6f))
                .AppendInterval(_autoDismissDelay)
                .Append(_messageText.DOFade(0f, _fadeDuration))
                .SetAutoKill(false)
                .Pause()
                .OnComplete(() => _messageText.gameObject.SetActive(false));
        }
        _msgTextSeq?.Restart();

        highlightOther = msg.Data as Graphic;
        if (highlightOther == null)
            return;

        Color otherOriginalColor = GetOriginalColor(highlightOther);

        _highlightOtherElmntSeq?.Kill();
        highlightOther.color = otherOriginalColor;

        _highlightOtherElmntSeq = DOTween.Sequence(highlightOther);
        _highlightOtherElmntSeq.Append(highlightOther.DOColor(pinkColor, 0.3f))
            .AppendInterval(3f)
            .Append(highlightOther.DOColor(otherOriginalColor, 0.3f))
            .OnComplete(() => highlightOther = null);
        _highlightOtherElmntSeq.PlayForward();
    }

    private void HidePopup() {
        _msgTextSeq?.Rewind();
        _messageText.color = GetOriginalColor(_messageText);
        _messageText.gameObject.SetActive(false);
        
        if(highlightOther == null) return;
        highlightOther.color = GetOriginalColor(highlightOther);
    }

    /// <summary>
    /// Returns the original color of a Graphic, caching it on first access
    /// to allow safe restoration after highlight animations
    /// </summary>
    private Color GetOriginalColor(Graphic graphic) {
        if(graphic == null) return Color.white;

        int id = graphic.GetInstanceID();

        if(!originalColorCache.ContainsKey(id))
            originalColorCache.Add(id, graphic.color);

        return originalColorCache[id];
    }

    /// <summary>
    /// Hides the graphic and caches its original color before any animations run.
    /// Called once in Awake to set the base state
    /// </summary>
    private void InitGraphic(Graphic graphic) {
        if (graphic == null)
            return;

        graphic.gameObject.SetActive(false);
        int instanceID = graphic.GetInstanceID();
        if (!originalColorCache.ContainsKey(instanceID))
            originalColorCache.Add(instanceID, graphic.color);

        Color color = graphic.color;
        Color transparent = new Color(color.r, color.g, color.b, 0f);
        graphic.color = transparent;
    }
}