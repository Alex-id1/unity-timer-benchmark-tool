using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Exports aggregated suite results to a wide-format CSV.
/// One row per scenario. Сolumns contain config params + mean/stddev/min/max/median/p95 per metric
/// </summary>
public class SuiteCsvReporter {
    private static readonly string[] Headers = {"Driver", "TimerType", "Instances", "Duration", "Intervals", "FPS_Mean", "FPS_StdDev", "FPS_Min",
        "FPS_Max", "FPS_Median", "FPS_P95", "GC_Mean", "GC_StdDev", "GC_Min", "GC_Max", "GC_Median", "GC_P95", "CPU_Mean", "CPU_StdDev", "CPU_Min",
        "CPU_Max", "CPU_Median", "CPU_P95"
    };

    public void Save(List<AggregatedResult> results, string path) {
        var sb = new StringBuilder(65536);
        sb.AppendLine(string.Join(",", Headers));

        foreach (var r in results)
            sb.AppendLine(BuildRow(r));

        File.WriteAllText(path, sb.ToString());
    }

    private static string BuildRow(AggregatedResult r) {
        var c = r.Config;
        return string.Join(",", new[] {
            c.TimerDriver.ToString(),
            c.TimerType.ToString(),
            c.NumOfInstances.ToString(),
            c.Duration.ToString(CultureInfo.InvariantCulture),
            c.NumOfIntervals.ToString(),

            F(r.Fps.Mean), F(r.Fps.StdDev), F(r.Fps.Min), F(r.Fps.Max), F(r.Fps.Median), F(r.Fps.P95), F(r.Gc.Mean), F(r.Gc.StdDev), F(r.Gc.Min), F(r.Gc.Max),
            F(r.Gc.Median), F(r.Gc.P95), F(r.CpuTime.Mean), F(r.CpuTime.StdDev), F(r.CpuTime.Min), F(r.CpuTime.Max), F(r.CpuTime.Median), F(r.CpuTime.P95)
        });
    }

    private static string F(double v) => v.ToString(CultureInfo.InvariantCulture);
}