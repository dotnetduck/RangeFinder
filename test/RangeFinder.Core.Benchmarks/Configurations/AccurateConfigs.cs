using BenchmarkDotNet.Jobs;

namespace RangeFinder.Core.Benchmarks;

/// <summary>
/// Accurate execution config - for comprehensive performance analysis
/// Uses thorough iterations and comprehensive datasets
/// </summary>
public class AccurateConfig : BenchmarkConfigBase
{
    public override string ConfigurationMode => "accurate";

    protected override void ConfigureJob()
    {
        AddJob(Job.Default
            .WithWarmupCount(10)     // Thorough warmup
            .WithIterationCount(15)  // More iterations for accuracy
            .WithMaxIterationCount(30)
            .WithLaunchCount(3));    // Multiple launches for statistical significance
    }

    protected override void ConfigureValidators()
    {
        // Keep all default validators for accurate benchmarks
        // This ensures proper validation and warnings for statistical significance
    }
}
