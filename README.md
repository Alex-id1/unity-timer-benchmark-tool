<img src="docs/icon.png" width="72" align="left"/>

# Unity Timer Benchmark Tool

> A Unity benchmarking and performance analysis tool (IL2CPP standalone) for profiling timer implementations, GC allocations, and runtime overhead under controlled workloads

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Unity](https://img.shields.io/badge/Unity-2022.3-black)
![IL2CPP](https://img.shields.io/badge/build-IL2CPP-orange)
![License](https://img.shields.io/badge/license-MIT-green)

---

![demo](docs/demo.gif)

| Main UI | Results |
|---|---|
| ![Main UI](docs/ss1.png) | ![Results](docs/ss2.png) |

---

## Motivation

Choosing a timer implementation in Unity is not an obvious decision. RX is syntactically convenient, coroutines are familiar, Update-loop is predictable. But how do they behave under load?

This tool was built to answer practical questions:
- For a game with hundreds of units each using a timer - which implementation produces less GC pressure?
- Is there a CPU time difference between approaches at scale?
- How stable is FPS when running 1000 timers concurrently?

Benchmark results are available at [unity-timer-benchmark](https://github.com/Alex-id1/unity-timer-benchmark).

---

## Technical Focus

- High-frequency timer benchmarking
- GC allocation tracking
- Multi-scenario workload simulation
- Statistical sampling (Mean, StdDev, Median, P95)
- Decoupled benchmark pipeline
- Reactive UI updates via UniRx
- Composition Root / DI-based architecture

---

## Architecture

```mermaid
graph LR
    AppInstaller["🔧 AppInstaller\n(Composition Root)"]

    subgraph Presentation
        BP["BenchmarkPresenter\n(Orchestrator)"]
        BV["BenchmarkView"]
        RV["ReporterView"]
    end

    subgraph Runners
        SR["SingleBenchmarkRunner"]
        SuR["SuiteBenchmarkRunner"]
    end

    subgraph Timers
        ITF["ITimerFactory"]
        RX["RxTimer"]
        CO["CoroutineTimer"]
        UP["UpdateTimer"]
    end

    subgraph Metrics
        MC["MetricsCollector"]
    end

    subgraph Views["Decoupled Views (MessageBroker)"]
        BSV["BlurScreenView"]
        PV["PopupView"]
        CV["ChartView"]
    end

    AppInstaller --> BP
    AppInstaller --> BV
    AppInstaller --> ITF
    AppInstaller --> MC

    BP --> SR
    BP --> SuR
    BP --> BV
    BP --> RV
    BP -->|publish| BSV
    BP -->|publish| PV
    BP -->|publish| CV

    SR --> ITF
    SR --> MC
    SuR --> ITF
    SuR --> MC

    ITF --> RX
    ITF --> CO
    ITF --> UP
```

The project follows the **MVP** (Model-View-Presenter) pattern: views are passive and contain no logic, `BenchmarkPresenter` is the sole orchestrator - it owns all services, subscribes to view events and manages the runner lifecycle.

**AppInstaller** acts as a Composition Root for DI (Dependency Injection) - the single place where all dependencies are created and wired together.

**`ITimer` / `ITimerFactory` - Strategy.** All three implementations are hidden behind a single interface. Benchmark code has no knowledge of which driver it works with: swapping RX -> Coroutine -> Update requires zero changes in runner code. The factory creates the required implementation by `TimerDriver` enum.

**`BenchmarkRunnerBase` - Template Method.** Shared logic (timer spawning, disposal, FPS cache) is extracted to a base class. `SingleBenchmarkRunner` and `SuiteBenchmarkRunner` implement only their execution strategy, with no knowledge of the UI.

**Events - two levels of coupling.** Direct C# events (`OnCompleted`, `OnFpsUpdated`) are used where the relationship between components is clear and justified. `MessageBroker` (UniRx) is used selectively - only for events between components that have no reason to know about each other (blur, popup, chart creation).

**`UpdateTimerRunner` + `ITimerTask`.** Update-loop timers avoid coroutine overhead by maintaining a flat `List<ITimerTask>` ticked every frame. New tasks are buffered in a pending list to avoid modifying the active list mid-iteration.

---

## Metrics

`MetricsCollector` captures three measurements:
- **FPS** - `1 / Time.deltaTime` sampled every 0.25s
- **GC** - `GC.GetTotalMemory(false)` in MB
- **CPU** - `ProfilerRecorder` Main Thread in nanoseconds

Collection follows the `Begin() -> Tick() x N -> Complete()` pattern, producing a `MetricsSample[]` with full statistics (Mean, StdDev, Min, Max, Median, P95).

---

## Why Not UniRx Timers Everywhere?

UniRx `Observable.Timer` / `Observable.Interval` are convenient but create observable chains and subscriptions that generate GC allocations on each firing. At scale (500-1000 concurrent timers) this becomes measurable. Coroutine and Update-loop implementations avoid this overhead - the benchmark exists to quantify the difference.

---

## Details

- **`targetFrameRate`** switches between `30` (idle) and `-1` (unlimited) on benchmark start and completion - preventing GPU warmup before measurements begin
- **Blur overlay** is implemented via a shader in the `Resources` folder - the only way to guarantee its inclusion in an IL2CPP build without an explicit scene reference
- **Suite mode** runs 60 configurations sequentially via coroutines with cooldown pauses between scenarios

---

## Tech Stack

| | |
|---|---|
| Engine | Unity 2022.3 LTS |
| Language | C# |
| Timer library | UniRx |
| Charts | XCharts 3.x |
| Tweening | DOTween |
| File dialogs | StandaloneFileBrowser |
| Build | IL2CPP, Windows x64 |

---

## License

MIT
