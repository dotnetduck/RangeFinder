using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace RangeFinder.Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
public class PointQueryBenchmarks : AbstractRangeFinderBenchmark
{
    protected override int DatasetSize =>
        int.Parse(Environment.GetEnvironmentVariable("BENCHMARK_DATASET_SIZE") ?? "100000");

    protected override int QueryCount =>
        int.Parse(Environment.GetEnvironmentVariable("BENCHMARK_QUERY_COUNT") ?? "25");

    protected override DatasetCharacteristic Characteristic =>
        Enum.Parse<DatasetCharacteristic>(Environment.GetEnvironmentVariable("BENCHMARK_CHARACTERISTIC") ?? "Uniform");

    [Benchmark(Baseline = true)]
    public int IntervalTree_PointQuery()
    {
        return ExecuteIntervalTreePointQueries().Count;
    }

    [Benchmark]
    public int RangeFinder_PointQuery()
    {
        return ExecuteRangeFinderPointQueries().Count;
    }
}
