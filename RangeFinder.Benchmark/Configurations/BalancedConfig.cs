using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Configs;

namespace RangeFinder.Benchmarks;

/// <summary>
/// Balanced execution config - balanced precision and speed for reasonable feedback
/// Uses minimal iterations but provides reliable results (~2 minutes)
/// </summary>
public class BalancedConfig : BenchmarkConfigBase
{
    public override string ConfigurationMode => "balanced";

    protected override void ConfigureJob()
    {
        // Force minimal iterations for fast development feedback
        AddJob(Job.Default
            .WithWarmupCount(1)
            .WithIterationCount(1)
            .WithUnrollFactor(1)
            .WithLaunchCount(1)
            .WithInvocationCount(1));   // Single invocation per iteration

        // Suppress warnings about low iteration counts
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }

    protected override void ConfigureValidators()
    {
        // Remove default validators that enforce minimum iteration counts
        AddValidator(JitOptimizationsValidator.DontFailOnError);
        AddValidator(ExecutionValidator.DontFailOnError);
    }
}
