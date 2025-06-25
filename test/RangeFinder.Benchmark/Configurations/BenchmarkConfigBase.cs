using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Loggers;

namespace RangeFinder.Benchmark;

/// <summary>
/// Abstract base class for benchmark configurations
/// Provides common configuration elements and ensures consistent setup
/// </summary>
public abstract class BenchmarkConfigBase : ManualConfig
{
    protected BenchmarkConfigBase()
    {
        // Configure job with derived class settings
        ConfigureJob();

        // Configure validators with derived class settings
        ConfigureValidators();

        // Common configuration for all benchmark configs
        AddLogger(ConsoleLogger.Default);
        AddColumnProvider(DefaultColumnProviders.Instance);
        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

        // Export results in standard formats
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(CsvExporter.Default);
    }

    /// <summary>
    /// Configure the benchmark job (iterations, warmup, etc.)
    /// Must be implemented by derived classes
    /// </summary>
    protected abstract void ConfigureJob();

    /// <summary>
    /// Configure validators for the benchmark
    /// Must be implemented by derived classes
    /// </summary>
    protected abstract void ConfigureValidators();

    /// <summary>
    /// Get the configuration mode name for parameter selection
    /// Must be implemented by derived classes
    /// </summary>
    public abstract string ConfigurationMode { get; }
}
