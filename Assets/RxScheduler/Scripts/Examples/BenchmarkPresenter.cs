using System;
using System.Collections;
using UniRx;
using UnityEngine;

/// <summary>
/// Orchestrator and single source of truth for all benchmark dependencies.
/// Owns all shared services and views; delegates execution to runners.
/// Single point of FPS calculation - broadcasts via OnFpsUpdated to all subscribers
/// </summary>
public class BenchmarkPresenter : IDisposable {
    public static event Action<float> OnFpsUpdated;

    private readonly ITimerFactory _timerFactory;
    private readonly IMetricsCollector _metrics;
    private readonly UpdateTimerRunner _runner;
    private readonly BenchmarkView _benchmarkView;
    private readonly ReporterView _reporterView;
    private readonly ChartView _chartView;
    private readonly IReporter _reporter;
    private readonly SuiteCsvReporter _suiteReporter;

    private readonly SingleBenchmarkRunner _singleRunner;
    private readonly SuiteBenchmarkRunner _suiteRunner;

    private IDisposable _fpsIntervalDisp;
    private bool _lastWasSuite;

    private BenchmarkContext Context => new BenchmarkContext(_timerFactory, _metrics, _runner);

    private const float FpsInterval = 0.25f;
    private const float closeMoveTime = 0.3f;
    private const int IdleFrameRate = 30;
    private const int BenchmarkFrameRate = -1;

    private const string ConfigNullErrorStr = "StartBenchmark: config is null";
    private const string SelectMetricStr = "Please select at least one metric";
    private const string SetInstancesNumStr = "Set timer instance count";
    private const string SetIntervalsNumStr = "Set number of intervals";
    private const string DurationEmptyStr = "Set duration value";
    private const string NothingToSaveStr = "Metrics data is empty - nothing to save";
    private const string BenchmarkUiMonitorStr = "[BenchmarkUiMonitor] ";

    public BenchmarkPresenter(
        BenchmarkView benchmarkView, ReporterView reporterView, ChartView chartView,
        ITimerFactory timerFactory, IMetricsCollector metrics, UpdateTimerRunner runner,
        IReporter reporter, SuiteCsvReporter suiteReporter) {

        _timerFactory = timerFactory;
        _metrics = metrics;
        _runner = runner;
        _benchmarkView = benchmarkView;
        _reporterView = reporterView;
        _chartView = chartView;
        _reporter = reporter;
        _suiteReporter = suiteReporter;

        _singleRunner = new SingleBenchmarkRunner();
        _suiteRunner = new SuiteBenchmarkRunner();

        Application.targetFrameRate = IdleFrameRate;
        InitFpsTimer(TimerDriver.RX);

        BenchmarkView.OnRunSingleBtnPressed += OnRunSingleClicked;
        BenchmarkView.OnRunSuiteBtnPressed += OnRunSuiteClicked;
        BenchmarkView.OnTimerDriverChanged += OnTimerDriverChanged;
        ReporterView.OnSaveBenchmarkBtnPressed += OnSaveClicked;

        SingleBenchmarkRunner.OnCompleted += OnSingleCompleted;
        SuiteBenchmarkRunner.OnCompleted += OnSuiteCompleted;
    }

    // -----------------------------------------
    //  FPS
    // -----------------------------------------
    private void InitFpsTimer(TimerDriver driver) {
        _fpsIntervalDisp?.Dispose();
        _fpsIntervalDisp = _timerFactory.Get(driver).Interval(
            FpsInterval,
            () => OnFpsUpdated?.Invoke(Mathf.Round((1f / Time.deltaTime) * 10f) / 10f)
        );
    }

    private void OnTimerDriverChanged(TimerDriver driver) => InitFpsTimer(driver);

    // -----------------------------------------
    //  Shared stop logic
    // -----------------------------------------
    // Stops all active runners and resets view. Returns true if anything was running
    private bool TryStopIfRunning() {
        bool singleRunning = _singleRunner.IsRunning;
        bool suiteRunning = _suiteRunner.IsRunning;
        if (!singleRunning && !suiteRunning) return false;

        if (singleRunning) _singleRunner.Stop();
        if (suiteRunning) _suiteRunner.Stop();
        Application.targetFrameRate = IdleFrameRate;
        _benchmarkView.ShowSingleRunningState(false);
        _benchmarkView.ShowSuiteRunningState(false);
        return true;
    }

    // -----------------------------------------
    //  Single benchmark
    // -----------------------------------------
    private void OnRunSingleClicked() => _runner.StartCoroutine(OnRunSingleCoroutine());

    private IEnumerator OnRunSingleCoroutine() {
        MessageBroker.Default.Publish(RxMsg.Create(RxMsgType.HIDE_POPUP));
        _reporterView.CloseSaveUIGroup(closeMoveTime, 0);
        yield return new WaitForSeconds(closeMoveTime);

        if (TryStopIfRunning()) yield break;

        BenchmarkConfig config = _benchmarkView.BuildConfig();

        if (config == null) { ShowPopupWithHighlight(ConfigNullErrorStr); yield break; }

        if (!config.IncludeFps & !config.IncludeGc & !config.IncludeCpuTime) {
            ShowPopupWithHighlight(SelectMetricStr, PopupMsgType.ERROR_NO_METRICS_SELECTED); yield break;
        }
        if (config.NumOfInstances == 0) {
            ShowPopupWithHighlight(SetInstancesNumStr, PopupMsgType.ERROR_ZERO_TIMER_INSTANCES); yield break;
        }
        if (config.Duration == 0) {
            ShowPopupWithHighlight(DurationEmptyStr, PopupMsgType.ERROR_ZERO_DURATION); yield break;
        }
        if (config.TimerType == TimerType.INTERVAL && config.NumOfIntervals == 0) {
            ShowPopupWithHighlight(SetIntervalsNumStr, PopupMsgType.ERROR_ZERO_INTERVAL_NUMBER); yield break;
        }

        Application.targetFrameRate = BenchmarkFrameRate;
        _benchmarkView.ShowSingleRunningState(true);
        _singleRunner.Run(Context, config);
    }

    private void OnSingleCompleted(BenchmarkResult result) {
        Application.targetFrameRate = IdleFrameRate;
        _lastWasSuite = false;
        _benchmarkView.ShowSingleRunningState(false);
        MessageBroker.Default.Publish(RxMsg.Create(RxMsgType.CREATE_CHART, result));
        _reporterView.OpenSaveBenchmarkToFilePopup(closeMoveTime, 0.5f);
    }

    // -----------------------------------------
    //  Suite benchmark
    // -----------------------------------------
    private void OnRunSuiteClicked() => _runner.StartCoroutine(OnRunSuiteCoroutine());

    private IEnumerator OnRunSuiteCoroutine() {
        MessageBroker.Default.Publish(RxMsg.Create(RxMsgType.HIDE_POPUP));
        _reporterView.CloseSaveUIGroup(closeMoveTime, 0);
        yield return new WaitForSeconds(closeMoveTime);

        if (TryStopIfRunning()) yield break;

        Application.targetFrameRate = BenchmarkFrameRate;
        _benchmarkView.ShowSuiteRunningState(true);
        _suiteRunner.Run(Context);
    }

    private void OnSuiteCompleted() {
        Application.targetFrameRate = IdleFrameRate;
        _lastWasSuite = true;
        _benchmarkView.ShowSuiteRunningState(false);
        _reporterView.OpenSaveBenchmarkToFilePopup(closeMoveTime, 0.5f);
    }

    // -----------------------------------------
    //  Save
    // -----------------------------------------
    private void OnSaveClicked(string path) {
        if (_lastWasSuite) {
            if (_suiteRunner.LastResult == null) {
                LogAndShowPopup(NothingToSaveStr);
                return;
            }
            _suiteReporter.Save(_suiteRunner.LastResult, path);
        } else {
            if (_singleRunner.LastResult == null) {
                LogAndShowPopup(NothingToSaveStr);
                return;
            }
            _reporter.Save(_singleRunner.LastResult, path);
            _chartView.SaveScreenshot(System.IO.Path.ChangeExtension(path, ".png"));
        }
    }

    // -----------------------------------------
    //  Helpers
    // -----------------------------------------
    private void LogAndShowPopup(string text) {
        Debug.LogError(BenchmarkUiMonitorStr + text);
        MessageBroker.Default.Publish(RxMsg.Create(RxMsgType.SHOW_POPUP, text));
    }

    private void ShowPopupWithHighlight(string text, PopupMsgType msgType = default) {
        Debug.LogError(BenchmarkUiMonitorStr + text);
        MessageBroker.Default.Publish(RxMsg.Create(RxMsgType.SHOW_POPUP, text, _benchmarkView.GetGraphic(msgType)));
    }

    // -----------------------------------------
    //  Lifecycle
    // -----------------------------------------
    public void Dispose() {
        _fpsIntervalDisp?.Dispose();

        BenchmarkView.OnRunSingleBtnPressed -= OnRunSingleClicked;
        BenchmarkView.OnRunSuiteBtnPressed -= OnRunSuiteClicked;
        BenchmarkView.OnTimerDriverChanged -= OnTimerDriverChanged;
        ReporterView.OnSaveBenchmarkBtnPressed -= OnSaveClicked;

        SingleBenchmarkRunner.OnCompleted -= OnSingleCompleted;
        SuiteBenchmarkRunner.OnCompleted -= OnSuiteCompleted;

        _singleRunner.Dispose();
        _suiteRunner.Dispose();
    }
}