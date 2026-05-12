
using System.Collections.Generic;

/// <summary>
/// Immutable snapshot of a completed benchmark run.
/// Holds the configuration it was run with and the raw per-interval metric samples.
/// Passed to reporters and charts
/// </summary>
public class BenchmarkResult {
    public BenchmarkConfig Config { get; }
    public List<double> Fps { get; }
    public List<double> Gc { get; }
    public List<double> CpuTime { get; }

    public BenchmarkResult(BenchmarkConfig config, List<double> fps, List<double> gc, List<double> cpuTime){
        Config = config;
        Fps = fps;
        Gc = gc;
        CpuTime = cpuTime;
    }
}