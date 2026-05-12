using System;
using System.Collections.Generic;
using Unity.Profiling;


/// <summary>
/// IMetricsCollector implementation that collects FPS, GC memory and CPU time at each benchmark interval.
/// CPU time is measured via Unity's ProfilerRecorder on the Main Thread category.
/// All lists are reused across runs - Begin() resets them
/// </summary>
public class MetricsCollector: IMetricsCollector {
    private BenchmarkConfig _config; // determines which metrics are sampled in Tick()

    // Per-interval sample lists provided to BenchmarkResult on Complete()
    private readonly List<double> _fps = new();
    private readonly List<double> _gc = new();
    private readonly List<double> _cpuTime = new();

    private ProfilerRecorder _cpuRecorder; // ProfilerRecorder for Main Thread nanoseconds. Started at Begin(), disposed at Complete()

    public void Begin(BenchmarkConfig config) {
        _config = config;

        _fps.Clear();
        _gc.Clear();
        _cpuTime.Clear();

        if (_cpuRecorder.Valid)
            _cpuRecorder.Dispose();

        _cpuRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Internal, "Main Thread", 60);
    }

    public void Tick(float curFps) {
        if(_config.IncludeFps) {
            //double fps = Math.Round((1f / Time.deltaTime) * 10.0) / 10.0;
            //_fps.Add(fps);
            _fps.Add(curFps);
        }

        if(_config.IncludeGc) {
            double mb = GC.GetTotalMemory(false) / 1_048_576.0; // no forced garbage collection; reads the current heap size
            double gc = Math.Round(mb * 10.0) / 10.0;
            _gc.Add(gc);
        }

        if(_config.IncludeCpuTime && _cpuRecorder.Valid) {
            long sum = 0;
            foreach(var sample in _cpuRecorder.ToArray())
                sum += sample.Value;

            //converts values from nanoseconds to milliseconds and computes the average across all recorder samples
            double avgMs = sum / (double)_cpuRecorder.Count / 1_000_000.0;
            double cpuMs = Math.Round(avgMs * 10.0) / 10.0;
            _cpuTime.Add(cpuMs);
        }
    }

    public BenchmarkResult Complete() {
        _cpuRecorder.Dispose();
        return new BenchmarkResult(_config, _fps, _gc, _cpuTime);
    }
}