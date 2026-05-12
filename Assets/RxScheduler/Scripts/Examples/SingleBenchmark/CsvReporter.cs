using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// IReporter implementation that serializes a single BenchmarkResult to CSV.
/// Each enabled metric becomes a column. Rows are per-interval samples.
/// Only columns with data are written, empty metrics are skipped
/// </summary>
public class CsvReporter: IReporter {
    public void Save(BenchmarkResult result, string path) {
        var columns = BuildColumns(result);

        if(columns.Count == 0)
            return;

        var sb = new StringBuilder(8192);

        sb.AppendLine(string.Join(",", columns.Select(c => c.header)));

        int maxRows = columns.Max(c => c.data.Count);

        for(int i = 0; i < maxRows; i++) {
            var row = new string[columns.Count];

            for(int j = 0; j < columns.Count; j++) {
                var list = columns[j].data;
                row[j] = i < list.Count
                    ? list[i].ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : string.Empty;
            }

            sb.AppendLine(string.Join(",", row));
        }

        File.WriteAllText(path, sb.ToString());
    }

    // Builds the column list from the result (only includes metrics that were enabled in config and have collected data)
    private List<(string header, IReadOnlyList<double> data)> BuildColumns(BenchmarkResult result) {
        var columns = new List<(string, IReadOnlyList<double>)>();

        if(result.Config.IncludeFps && result.Fps.Count > 0)
            columns.Add((MetricsDataType.FPS.ToString(), result.Fps));

        if(result.Config.IncludeGc && result.Gc.Count > 0)
            columns.Add((MetricsDataType.GC_ALLOCATION.ToString(), result.Gc));

        if(result.Config.IncludeCpuTime && result.CpuTime.Count > 0)
            columns.Add((MetricsDataType.CPU_TIME.ToString(), result.CpuTime));

        return columns;
    }
}