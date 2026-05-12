using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Reads benchmark configuration from UI controls.
/// Owns all input validation - BenchmarkPresenter just calls BuildConfig()
/// </summary>
public class BenchmarkView : MonoBehaviour {
    [Header("Controls")]
    [SerializeField] private Button _runSingleBenchmarkButton;
    [SerializeField] private Button _runSuiteBenchmarkButton;

    [Header("Types")]
    [SerializeField] private Dropdown _timerDriverDropdown;
    [SerializeField] private Dropdown _timerTypeDropdown;

    [Header("Parameters")]
    [SerializeField] private InputField _instancesNumInputField;
    [SerializeField] private InputField _durationInputField;
    [SerializeField] private InputField _intervalsNumInputField;
    [SerializeField] private CanvasGroup numOfIntervalsCanvasGroup;

    [Header("Metrics toggles")]
    [SerializeField] private Toggle _includeFpsToggle;
    [SerializeField] private Toggle _includeGcToggle;
    [SerializeField] private Toggle _includeCpuTimeToggle;
    [SerializeField] private RectTransform metricsTogglesBlockRT;
    [SerializeField] private Graphic includeMetricsTitleText;

    [Header("Info")]
    [SerializeField] private Text _testRunningText;
    [SerializeField] private Text curFpsText;
    private Text _startBenchmarkBtnText;
    private Text _runSuiteBtnText;

    // Defaults
    private int _instancesNum;
    private float _duration;
    private int _intervalsNum;
    private TimerType _timerType = default;
    private TimerDriver _timerDriver = default;

    public static event Action OnRunSingleBtnPressed;
    public static event Action OnRunSuiteBtnPressed;
    public static event Action<TimerDriver> OnTimerDriverChanged;

    private const string startStr = "START";
    private const string stopStr = "STOP";
    private const string runSuiteStr = "RUN SUITE";
    private const string suiteRunningStr = "RUNNING...";

    private Dictionary<int, Color> _originalColorCache = new Dictionary<int, Color>();

    // -----------------------------------------
    //  Unity
    // -----------------------------------------
    private void Awake() {
        Init();

        _runSingleBenchmarkButton.OnClickAsObservable().Subscribe(_ => OnRunSingleBtnPressed?.Invoke()).AddTo(this);
        _runSuiteBenchmarkButton.OnClickAsObservable().Subscribe(_ => OnRunSuiteBtnPressed?.Invoke()).AddTo(this);
        _instancesNumInputField.OnValueChangedAsObservable().Subscribe(s => _instancesNum = StringToInt(s)).AddTo(this);
        _durationInputField.OnValueChangedAsObservable().Subscribe(s => _duration = StringToInt(s)).AddTo(this);
        _intervalsNumInputField.OnValueChangedAsObservable().Subscribe(s => _intervalsNum = StringToInt(s)).AddTo(this);

        _timerDriverDropdown.OnValueChangedAsObservable().Subscribe(s => {
            int val = _timerDriverDropdown.value;
            _timerDriver = ParseEnum<TimerDriver>(_timerDriverDropdown.options[val].text);
            OnTimerDriverChanged?.Invoke(_timerDriver);
        }).AddTo(this);

        _timerTypeDropdown.OnValueChangedAsObservable().Subscribe(s => {
            int val = _timerTypeDropdown.value;
            _timerType = ParseEnum<TimerType>(_timerTypeDropdown.options[val].text);
            UpdateMetricsTogglesBlock(_timerType);
        }).AddTo(this);
    }

    private void OnEnable() => BenchmarkPresenter.OnFpsUpdated += UpdateFps;
    private void OnDisable() => BenchmarkPresenter.OnFpsUpdated -= UpdateFps;

    private void Init() {
        _startBenchmarkBtnText = _runSingleBenchmarkButton.GetComponentInChildren<Text>();
        _runSuiteBtnText = _runSuiteBenchmarkButton.GetComponentInChildren<Text>();

        _instancesNumInputField.text = _instancesNum.ToString();
        _durationInputField.text = _duration.ToString();
        _intervalsNumInputField.text = _intervalsNum.ToString();

        var _timerDriverOptions = Enum.GetNames(typeof(TimerDriver)).ToList();
        _timerDriverDropdown.AddOptions(_timerDriverOptions);

        var timerTypeOptions = Enum.GetNames(typeof(TimerType)).ToList();
        _timerTypeDropdown.AddOptions(timerTypeOptions);
    }

    /// <summary>
    /// Builds and validates BenchmarkConfig from current UI state.
    /// Returns null and logs error if validation fails.
    /// </summary>
    public BenchmarkConfig BuildConfig() {
        return new BenchmarkConfigBuilder()
            .WithDriver(_timerDriver)
            .WithType(_timerType)
            .WithInstances(_instancesNum)
            .WithDuration(_duration)
            .WithIntervals(_intervalsNum)
            .IncludeFps(_includeFpsToggle.isOn)
            .IncludeGc(_includeGcToggle.isOn)
            .IncludeCpuTime(_includeCpuTimeToggle.isOn)
            .Build();
    }

    /// <summary>
    /// Changes Start button text START/STOP and shows the running indicator
    /// </summary>
    public void ShowSingleRunningState(bool isRunning) {
        _startBenchmarkBtnText.text = isRunning ? stopStr : startStr;
        _runSuiteBenchmarkButton.interactable = !isRunning;
        ShowTestRunningText(isRunning);
    }

    /// <summary>
    /// Changes Run Suite button text and disables Start to prevent concurrent runs
    /// </summary>
    public void ShowSuiteRunningState(bool isRunning) {
        _runSuiteBtnText.text = isRunning ? suiteRunningStr : runSuiteStr;
        _runSingleBenchmarkButton.interactable = !isRunning;
        ShowTestRunningText(isRunning);
    }

    private Sequence _testRunningTextSeq;
    private void ShowTestRunningText(bool show) {
        if (show) {
            var textID = _testRunningText.GetInstanceID();
            if (!_originalColorCache.ContainsKey(textID))
                _originalColorCache.Add(textID, _testRunningText.color);

            _testRunningText.color = _originalColorCache[textID];
            _testRunningText.gameObject.SetActive(true);

            if (_testRunningTextSeq == null) {
                _testRunningTextSeq = DOTween.Sequence();
                _testRunningTextSeq.Append(_testRunningText.DOFade(0.9f, 0.6f))
                    .PrependInterval(1)
                    .Append(_testRunningText.DOFade(0f, 0.3f))
                    .Append(_testRunningText.DOFade(0.9f, 0.6f))
                    .AppendInterval(0.5f)
                    .Append(_testRunningText.DOFade(0f, 0.3f))
                    .Append(_testRunningText.DOFade(0.9f, 0.6f))
                    .AppendInterval(0.5f)
                    .Append(_testRunningText.DOFade(0f, 0.6f))
                    .AppendInterval(2)
                    .SetLoops(-1, LoopType.Restart);
            }
            _testRunningTextSeq?.PlayForward();
        } else {
            _testRunningTextSeq?.Pause();
            _testRunningTextSeq?.Rewind();
            _testRunningText.DOFade(0f, 0.3f).OnComplete(() => _testRunningText.gameObject.SetActive(false));
        }
    }

    // -----------------------------------------
    //  Callbacks
    // -----------------------------------------
    public void UpdateFps(float fps) => curFpsText.text = fps.ToString("F1");

    // -----------------------------------------
    //  UI helpers
    // -----------------------------------------
    private void UpdateMetricsTogglesBlock(TimerType timerType) {
        if (timerType is TimerType.INTERVAL) {
            metricsTogglesBlockRT.DOAnchorPosY(-433f, 0.3f).OnComplete(() =>
                numOfIntervalsCanvasGroup.DOFade(1f, 0.15f)
            );
        } else {
            numOfIntervalsCanvasGroup.DOFade(0f, 0.15f).OnComplete(() =>
                metricsTogglesBlockRT.DOAnchorPosY(-335f, 0.3f)
            );
        }
    }

    public Graphic GetGraphic(PopupMsgType type) => type switch {
        PopupMsgType.DEFAULT => null,
        PopupMsgType.ERROR_NO_METRICS_SELECTED => includeMetricsTitleText,
        PopupMsgType.ERROR_ZERO_TIMER_INSTANCES => _instancesNumInputField.GetComponent<Graphic>(),
        PopupMsgType.ERROR_ZERO_DURATION => _durationInputField.GetComponent<Graphic>(),
        PopupMsgType.ERROR_ZERO_INTERVAL_NUMBER => _intervalsNumInputField.GetComponent<Graphic>(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public int StringToInt(string text) {
        return !string.IsNullOrWhiteSpace(text) && int.TryParse(text, out var result) ? result : 0;
    }

    private static TEnum ParseEnum<TEnum>(string name) where TEnum : struct, Enum {
        if (!Enum.TryParse<TEnum>(name, true, out var value)) {
            Debug.LogError($"Error while parsing enum {typeof(TEnum)}. Default value returned");
            return default;
        }

        if (!Enum.IsDefined(typeof(TEnum), value)) {
            Debug.LogError($"Selected value is not defined in enum {typeof(TEnum)}. Default value returned");
            return default;
        }

        return value;
    }
}