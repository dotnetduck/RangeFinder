using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using IntervalTree;
using RangeFinder.Core;

namespace RangeFinder.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
public class ConstructionBenchmarks : AbstractRangeFinderBenchmark
{
    protected override int DatasetSize => 
        int.Parse(Environment.GetEnvironmentVariable("BENCHMARK_DATASET_SIZE") ?? "10000");
    
    protected override DatasetCharacteristic Characteristic => 
        Enum.Parse<DatasetCharacteristic>(Environment.GetEnvironmentVariable("BENCHMARK_CHARACTERISTIC") ?? "Uniform");
    protected override int QueryCount => 25; // Minimal queries just to keep objects alive
    protected override bool ShouldPreConstructDataStructures => false; // Don't pre-construct for construction benchmarks
    
    public override void Setup()
    {
        // Call base setup to generate data (but not construct data structures)
        base.Setup();
        
        // RANDOMIZE source data for construction benchmarks to avoid sorted data bias
        var random = new Random(RandomSeed + 42);
        for (int i = _sourceData.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (_sourceData[i], _sourceData[j]) = (_sourceData[j], _sourceData[i]);
        }
    }

    [Benchmark(Baseline = true)]
    public int IntervalTree_Construction()
    {
        // Construct IntervalTree with maximum public API optimizations
        var intervalTree = new IntervalTree<double, int>();
        
        // Step 1: Add all ranges (deferred tree building)
        var count = _sourceData.Count;
        for (int i = 0; i < count; i++)
        {
            var range = _sourceData[i];
            intervalTree.Add(range.Start, range.End, range.Value);
        }

        // Step 2: Force tree construction (equivalent to calling private Rebuild())
        // This triggers the actual tree building that IntervalTree defers
        _ = intervalTree.Min; // Forces Rebuild() to be called
        
        var result = intervalTree.Query(_sourceData[0].Start, _sourceData[0].End);
        return result.Count();
    }

    [Benchmark]
    public int RangeFinder_Construction()
    {
        // Construct RangeFinder from scratch using bulk constructor
        var rangeFinder = new RangeFinder<double, int>(_sourceData);
        
        var result = rangeFinder.QueryRanges(_sourceData[0].Start, _sourceData[0].End);
        return result.Count();
    }
}