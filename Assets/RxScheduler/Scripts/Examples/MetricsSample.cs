
/// <summary>
/// Immutable single-point measurement captured during a benchmark interval.
/// Represents a metric type with its sampled value
/// </summary>
public class MetricsSample {
    public MetricsDataType Type { get; }
    public double Value { get; }

    public MetricsSample(MetricsDataType type, double value) {
        Type = type;
        Value = value;
    }
}