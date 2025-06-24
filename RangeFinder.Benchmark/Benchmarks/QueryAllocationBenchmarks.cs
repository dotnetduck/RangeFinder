using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace RangeFinder.Benchmarks;

/// <summary>
/// Measures memory allocations during query execution by collecting results.
/// This isolates query allocation patterns from construction memory.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class QueryAllocationBenchmarks : AbstractRangeFinderBenchmark
{
    protected override int DatasetSize =>
        int.Parse(Environment.GetEnvironmentVariable("BENCHMARK_DATASET_SIZE") ?? "10000");

    protected override int QueryCount =>
        int.Parse(Environment.GetEnvironmentVariable("BENCHMARK_QUERY_COUNT") ?? "25");

    protected override DatasetCharacteristic Characteristic =>
        Enum.Parse<DatasetCharacteristic>(Environment.GetEnvironmentVariable("BENCHMARK_CHARACTERISTIC") ?? "Uniform");

    [Benchmark(Baseline = true)]
    public int IntervalTree_QueryAllocations()
    {
        // Execute queries and collect results to measure allocations
        var results = ExecuteIntervalTreeRangeQueries();
        return results.Count;
    }

    [Benchmark]
    public int RangeFinder_QueryAllocations()
    {
        // Execute queries and collect results to measure allocations
        var results = ExecuteRangeFinderRangeQueries();
        return results.Count;
    }
}
