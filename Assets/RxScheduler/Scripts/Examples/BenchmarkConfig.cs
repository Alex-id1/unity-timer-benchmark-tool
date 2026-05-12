/// <summary>
/// Immutable benchmark configuration- create via BenchmarkConfigBuilder
/// </summary>
public sealed class BenchmarkConfig {
    public TimerDriver TimerDriver { get; }
    public TimerType TimerType { get; }
    public int NumOfInstances { get; }
    public float Duration { get; }
    public int NumOfIntervals { get; }
    public bool IncludeFps { get; }
    public bool IncludeGc { get; }
    public bool IncludeCpuTime { get; }

    internal BenchmarkConfig(TimerDriver timerDriver, TimerType timerType, int numOfInstances, float duration, int numOfIntervals,
        bool includeFps, bool includeGc, bool includeCpuTime) {

        TimerDriver = timerDriver;
        TimerType = timerType;
        NumOfInstances = numOfInstances;
        Duration = duration;
        NumOfIntervals = numOfIntervals;
        IncludeFps = includeFps;
        IncludeGc = includeGc;
        IncludeCpuTime = includeCpuTime;
    }
}