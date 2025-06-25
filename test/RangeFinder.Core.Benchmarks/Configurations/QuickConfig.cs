using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Configs;

namespace RangeFinder.Core.Benchmarks;

/// <summary>
/// Quick execution config - ultra-fast execution for quick overview within 30 seconds
/// Uses absolute minimal iterations and reduced dataset combinations
/// </summary>
public class QuickConfig : BenchmarkConfigBase
{
    public override string ConfigurationMode => "quick";

    protected override void ConfigureJob()
    {
        // Ultra-minimal iterations for 2-minute completion
        AddJob(Job.Default
            .WithWarmupCount(0)         // No warmup for speed
            .WithIterationCount(1)      // Single iteration only
            .WithUnrollFactor(1)
            .WithLaunchCount(1)
            .WithInvocationCount(1));   // Single invocation per iteration

        // Suppress all warnings about low iteration counts
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }

    protected override void ConfigureValidators()
    {
        // Remove all validators that enforce minimum iteration counts
        AddValidator(JitOptimizationsValidator.DontFailOnError);
        AddValidator(ExecutionValidator.DontFailOnError);
    }
}
