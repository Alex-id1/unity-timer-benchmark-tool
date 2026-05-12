using System;
using System.Collections.Generic;

/// <summary>
/// Per-metric statistics computed from all samples across N repeated runs of one scenario
/// </summary>
public class MetricStats {
    public double Mean { get; }
    public double StdDev { get; }
    public double Min { get; }
    public double Max { get; }
    public double Median { get; }
    public double P95 { get; }

    private MetricStats(double mean, double stdDev, double min, double max, double median, double p95) {
        Mean = mean; StdDev = stdDev; Min = min; Max = max; Median = median; P95 = p95;
    }

    public static MetricStats Compute(List<double> samples) {
        if (samples == null || samples.Count == 0)
            return new MetricStats(0, 0, 0, 0, 0, 0);

        var sorted = new List<double>(samples);
        sorted.Sort();

        double sum = 0;
        foreach (var s in sorted) sum += s;
        double mean = sum / sorted.Count;

        double variance = 0;
        foreach (var s in sorted) variance += (s - mean) * (s - mean);
        double stdDev = Math.Sqrt(variance / sorted.Count);

        return new MetricStats(
            Math.Round(mean, 2),
            Math.Round(stdDev, 2),
            Math.Round(sorted[0], 2),
            Math.Round(sorted[sorted.Count - 1], 2),
            Math.Round(Percentile(sorted, 0.50), 2),
            Math.Round(Percentile(sorted, 0.95), 2)
        );
    }

    private static double Percentile(List<double> sorted, double p) {
        double index = p * (sorted.Count - 1);
        int lo = (int)Math.Floor(index);
        int hi = lo + 1;
        if (hi >= sorted.Count) return sorted[lo];
        return sorted[lo] + (index - lo) * (sorted[hi] - sorted[lo]);
    }
}

/// <summary>
/// Aggregates N BenchmarkResults for one config into mean/stddev/min/max/p95 per metric
/// </summary>
public class AggregatedResult {
    public BenchmarkConfig Config { get; }
    public MetricStats Fps { get; }
    public MetricStats Gc { get; }
    public MetricStats CpuTime { get; }

    public AggregatedResult(BenchmarkConfig config, List<BenchmarkResult> runs) {
        Config = config;

        var allFps = new List<double>();
        var allGc  = new List<double>();
        var allCpu = new List<double>();

        foreach (var r in runs) {
            allFps.AddRange(r.Fps);
            allGc.AddRange(r.Gc);
            allCpu.AddRange(r.CpuTime);
        }

        Fps = MetricStats.Compute(allFps);
        Gc = MetricStats.Compute(allGc);
        CpuTime = MetricStats.Compute(allCpu);
    }
}