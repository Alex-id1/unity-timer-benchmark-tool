using System;

/// <summary>
/// Fluent builder for BenchmarkConfig.
/// Validates domain constraints in Build() - UI validation stays in the presenter
/// </summary>
public sealed class BenchmarkConfigBuilder {
    private TimerDriver _driver;
    private TimerType _type;
    private int _instances;
    private float _duration;
    private int _intervals;
    private bool _includeFps = true;
    private bool _includeGc = true;
    private bool _includeCpuTime = true;

    public BenchmarkConfigBuilder WithDriver(TimerDriver driver) { _driver = driver; return this; }
    public BenchmarkConfigBuilder WithType(TimerType type) { _type = type; return this; }
    public BenchmarkConfigBuilder WithInstances(int count) { _instances = count; return this; }
    public BenchmarkConfigBuilder WithDuration(float seconds) { _duration = seconds; return this; }
    public BenchmarkConfigBuilder WithIntervals(int count) { _intervals = count; return this; }
    public BenchmarkConfigBuilder IncludeFps(bool include) { _includeFps = include; return this; }
    public BenchmarkConfigBuilder IncludeGc(bool include) { _includeGc = include; return this; }
    public BenchmarkConfigBuilder IncludeCpuTime(bool include) { _includeCpuTime = include; return this; }

    /// <summary>
    /// Builds the config - throws ArgumentOutOfRangeException if domain constraints are violated
    /// </summary>
    public BenchmarkConfig Build() {
        if (_instances < 0)
            throw new ArgumentOutOfRangeException(nameof(_instances), "NumOfInstances must be >= 0");
        if (_duration < 0f)
            throw new ArgumentOutOfRangeException(nameof(_duration), "Duration must be >= 0");

        return new BenchmarkConfig(_driver, _type, _instances, _duration, _intervals, _includeFps, _includeGc, _includeCpuTime);
    }
}