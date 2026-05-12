
public interface IReporter {
    /// <summary>Save benchmark results (CSV, JSON, etc.)</summary>
    void Save(BenchmarkResult result, string path);
}