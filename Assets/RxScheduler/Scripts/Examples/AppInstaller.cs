using UnityEngine;

/// <summary>
/// Composition Root. Creates all dependencies manually and wires them together.
/// This is the only place in the project where 'new' is called on service classes.
/// Must be executed before any other MonoBehaviour
/// </summary>
public class AppInstaller : MonoBehaviour {
    [Header("Presentation")]
    [SerializeField] private BenchmarkView _benchmarkView;
    [SerializeField] private ChartView _chartView;
    [SerializeField] private ReporterView _reporterView;
    [SerializeField] private PopupView _popupView;

    private BenchmarkPresenter _presenter;

    private void Awake() {
        UpdateTimerRunner runner = CreateRunner();

        ITimerFactory timerFactory = new TimerFactory(
            new RxTimer(),
            new UpdateTimer(runner),
            new CoroutineTimer(runner)
        );

        BenchmarkSavePathProvider savePath = new BenchmarkSavePathProvider();
        _reporterView.Init(savePath);

        _presenter = new BenchmarkPresenter(
            _benchmarkView,
            _reporterView,
            _chartView,
            timerFactory,
            new MetricsCollector(),
            runner,
            new CsvReporter(),
            new SuiteCsvReporter()
        );
    }

    private UpdateTimerRunner CreateRunner() {
        var go = new GameObject("UpdateTimerRunner");
        DontDestroyOnLoad(go);
        return go.AddComponent<UpdateTimerRunner>();
    }

    private void OnDestroy() {
        _presenter?.Dispose();
    }
}