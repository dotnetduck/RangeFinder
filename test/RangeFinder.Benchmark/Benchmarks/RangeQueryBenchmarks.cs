using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace RangeFinder.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
public class RangeQueryBenchmarks : AbstractRangeFinderBenchmark
{
    protected override int DatasetSize =>
        int.Parse(Environment.GetEnvironmentVariable("BENCHMARK_DATASET_SIZE") ?? "100000");

    protected override int QueryCount =>
        int.Parse(Environment.GetEnvironmentVariable("BENCHMARK_QUERY_COUNT") ?? "25");

    protected override DatasetCharacteristic Characteristic =>
        Enum.Parse<DatasetCharacteristic>(Environment.GetEnvironmentVariable("BENCHMARK_CHARACTERISTIC") ?? "Uniform");

    [Benchmark(Baseline = true)]
    public int IntervalTree_RangeQuery()
    {
        return ExecuteIntervalTreeRangeQueries().Count;
    }

    [Benchmark]
    public int RangeFinder_RangeQuery()
    {
        return ExecuteRangeFinderRangeQueries().Count;
    }
}
