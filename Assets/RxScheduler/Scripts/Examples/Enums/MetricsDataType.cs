/// <summary>
/// Identifies the type of metric captured in a MetricsSample.
/// Used by CsvReporter to map samples to CSV column headers
/// </summary>
public enum MetricsDataType {
    FPS,
    GC_ALLOCATION,
    CPU_TIME
}