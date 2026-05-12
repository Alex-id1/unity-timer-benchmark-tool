using System.Collections.Generic;

/// <summary>
/// Defines the full benchmark matrix for automated suite runs.
/// All scenarios are hardcoded so results are reproducible.
/// Adjust the constants below to change the scope of the test run
/// </summary>
public static class SuiteScenarioLibrary {
    // ---Matrix dimensions---------------------------------------------------
    private static readonly TimerDriver[] Drivers = {TimerDriver.RX, TimerDriver.COROUTINE, TimerDriver.UPDATE};

    private static readonly TimerType[] TimerTypes = {TimerType.DELAY, TimerType.INTERVAL, TimerType.TIMER, TimerType.COUNTDOWN};

    // Timer count per scenario - tests scaling from single timer to high concurrency
    private static readonly int[] Counts = { 1, 10, 100, 500, 1000 };

    // Duration used for all timer types(seconds)
    // For INTERVAL: this is the period between ticks
    private const float Duration = 2f;

    // How many ticks INTERVAL timers fire before stopping
    private const int IntervalTickCount = 10;

    // Statistical repeats - each scenario runs this many times and results are aggregated
    private const int Repeats = 5;

    // --Public API-----------------------------------------------------------

    /// <summary>
    /// Returns 3 drivers x 4 types x 5 counts = 60 scenarios, each repeated 5 times.
    /// Total runtime estimate: ~25 minutes on a standard machine
    /// </summary>
    public static IReadOnlyList<(BenchmarkConfig config, int repeats)> GetFullMatrix() {
        var result = new List<(BenchmarkConfig, int)>(
            Drivers.Length * TimerTypes.Length * Counts.Length);

        foreach (var driver in Drivers)
        foreach (var type in TimerTypes)
        foreach (var count in Counts) {
            var config = new BenchmarkConfigBuilder()
                .WithDriver(driver)
                .WithType(type)
                .WithInstances(count)
                .WithDuration(Duration)
                .WithIntervals(IntervalTickCount)
                .IncludeFps(true)
                .IncludeGc(true)
                .IncludeCpuTime(true)
                .Build();

            result.Add((config, Repeats));
        }

        return result;
    }

    /// <summary>
    /// Smaller matrix for quick smoke-testing: 3 drivers x 2 types x 3 counts = 18 scenarios.
    /// Total runtime estimate: ~8 minutes
    /// </summary>
    public static IReadOnlyList<(BenchmarkConfig config, int repeats)> GetSmokeMatrix() {
        var result = new List<(BenchmarkConfig, int)>();

        foreach (var driver in Drivers)
        foreach (var type in new[] { TimerType.DELAY, TimerType.INTERVAL })
        foreach (var count in new[] { 1, 100, 1000 }) {
            var config = new BenchmarkConfigBuilder()
                .WithDriver(driver)
                .WithType(type)
                .WithInstances(count)
                .WithDuration(Duration)
                .WithIntervals(IntervalTickCount)
                .IncludeFps(true)
                .IncludeGc(true)
                .IncludeCpuTime(true)
                .Build();

            result.Add((config, Repeats));
        }

        return result;
    }
}