using System;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Manages default folder for benchmark exports and builds suggested file names.
/// Folder is created on Desktop on first access and remembered between saves
/// </summary>
public sealed class BenchmarkSavePathProvider {
    private const string DefaultFolderName = "TimersBenchmarkTest";
    private const string FallbackTestName = "benchmark";
    private const string DateTimeFormat = "yyyy-MM-dd_HH-mm";

    private string _lastFolder;

    /// <summary>
    /// Returns the last used folder, or creates Desktop/TimersBenchmarkTest on first call
    /// </summary>
    public string DefaultFolder {
        get {
            if (string.IsNullOrEmpty(_lastFolder)) {
                _lastFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    DefaultFolderName);

                if (!Directory.Exists(_lastFolder))
                    Directory.CreateDirectory(_lastFolder);
            }
            return _lastFolder;
        }
    }

    /// <summary>
    /// Builds a default file name like "MyTest_2026-01-26_18-30.csv".
    /// Removes whitespace from testName, falls back to "benchmark" if empty
    /// </summary>
    public string BuildDefaultFileName(string testName, bool includeDateTime = true) {
        string cleaned = Regex.Replace(testName ?? "", @"\s+", "");
        if (string.IsNullOrEmpty(cleaned))
            cleaned = FallbackTestName;

        string suffix = includeDateTime ? $"_{DateTime.Now.ToString(DateTimeFormat)}" : "";
        return $"{cleaned}{suffix}.csv";
    }

    /// <summary>
    /// Remembers the directory of the chosen file path for the next save
    /// </summary>
    public void RememberFolder(string filePath) {
        if (!string.IsNullOrEmpty(filePath))
            _lastFolder = Path.GetDirectoryName(filePath);
    }
}