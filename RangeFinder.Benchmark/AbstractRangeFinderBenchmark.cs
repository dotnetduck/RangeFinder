using BenchmarkDotNet.Attributes;
using IntervalTree;
using RangeFinder.Core;
using RangeFinder.Generator;

namespace RangeFinder.Benchmarks;

/// <summary>
/// Abstract base class for RangeFinder performance benchmarks.
/// Provides common infrastructure for comparing RangeFinder against IntervalTree
/// across different dataset characteristics and query patterns.
/// </summary>
/// <remarks>
/// This class handles:
/// - Data generation with configurable characteristics (Uniform, Dense, Sparse, Temporal)
/// - Data structure construction for both RangeFinder and IntervalTree
/// - Result validation to ensure correctness before performance measurement
/// - Common query execution patterns for range and point queries
/// - Environment variable-based configuration for BenchmarkDotNet isolation
/// </remarks>
public abstract class AbstractRangeFinderBenchmark
{
    /// <summary>
    /// Source data ranges generated according to the specified dataset characteristic.
    /// Each range contains start/end positions and an integer value for identification.
    /// </summary>
    protected List<NumericRange<double, int>> _sourceData = null!;
    
    /// <summary>
    /// Query ranges used for testing range query performance.
    /// Generated independently from source data to avoid bias.
    /// </summary>
    protected List<NumericRange<double, object>> _queryRanges = null!;
    
    /// <summary>
    /// RangeFinder instance constructed from source data for performance testing.
    /// </summary>
    protected RangeFinder<double, int> _rangeFinder = null!;
    
    /// <summary>
    /// IntervalTree instance constructed from source data for comparison testing.
    /// Stores the entire NumericRange as the value for each interval.
    /// </summary>
    protected IntervalTree<double, NumericRange<double, int>> _intervalTree = null!;

    /// <summary>
    /// Number of ranges in the source dataset. Configured via BENCHMARK_DATASET_SIZE environment variable.
    /// </summary>
    protected abstract int DatasetSize { get; }
    
    /// <summary>
    /// Number of queries to execute in each benchmark. Configured via BENCHMARK_QUERY_COUNT environment variable.
    /// </summary>
    protected abstract int QueryCount { get; }
    
    /// <summary>
    /// Multiplier for query range length relative to average source range length.
    /// Larger values create queries that span more ranges. Default: 2.0
    /// </summary>
    protected virtual double QueryLengthMultiplier => 2.0;
    
    /// <summary>
    /// Random seed for reproducible data generation across benchmark runs.
    /// Different seeds used for source data vs query data to avoid correlation.
    /// </summary>
    protected virtual int RandomSeed => 42;
    
    /// <summary>
    /// Dataset characteristic determining the distribution pattern of generated ranges.
    /// Configured via BENCHMARK_CHARACTERISTIC environment variable.
    /// </summary>
    protected abstract DatasetCharacteristic Characteristic { get; }

    /// <summary>
    /// BenchmarkDotNet GlobalSetup method that prepares data and validates correctness
    /// before performance measurement begins.
    /// </summary>
    /// <remarks>
    /// Execution order:
    /// 1. Generate source data according to dataset characteristic
    /// 2. Generate query ranges for testing
    /// 3. Construct both RangeFinder and IntervalTree data structures
    /// 4. Validate that both implementations return identical results
    /// 
    /// If validation fails, throws InvalidOperationException to prevent
    /// performance measurement of incorrect implementations.
    /// </remarks>
    [GlobalSetup]
    public virtual void Setup()
    {
        GenerateSourceData();
        GenerateQueryRanges();
        if (ShouldPreConstructDataStructures)
        {
            ConstructDataStructures();
            ValidateResultCorrectness();
        }
    }
    
    /// <summary>
    /// Whether to pre-construct data structures during GlobalSetup.
    /// Set to false for construction benchmarks that measure build time.
    /// </summary>
    protected virtual bool ShouldPreConstructDataStructures => true;

    /// <summary>
    /// Generates source data ranges according to the specified dataset characteristic.
    /// Uses deterministic random generation for reproducible benchmarks.
    /// </summary>
    private void GenerateSourceData()
    {
        var random = new Random(RandomSeed);
        _sourceData = new List<NumericRange<double, int>>();

        // DEBUG: Log what characteristic is actually being used
        Console.WriteLine($"DEBUG: Generating {Characteristic} data for {DatasetSize} elements");

        switch (Characteristic)
        {
            case DatasetCharacteristic.Uniform:
                GenerateUniformData(random);
                break;
            case DatasetCharacteristic.Dense:
                GenerateDenseData(random);
                break;
            case DatasetCharacteristic.Sparse:
                GenerateSparseData(random);
                break;
            case DatasetCharacteristic.Temporal:
                GenerateTemporalData(random);
                break;
            default:
                GenerateRandomData(random); // Default to truly random
                break;
        }
        
        // DEBUG: Verify the generated data characteristics
        var stats = Analyzer.Analyze(_sourceData);
        Console.WriteLine($"DEBUG: {Characteristic} - {stats.FormatCharacteristics()}");
    }
    

    /// <summary>
    /// Generates uniformly distributed ranges across a large space.
    /// Provides baseline scenario with balanced overlap characteristics.
    /// </summary>
    private void GenerateUniformData(Random random)
    {
        // Truly random ranges (not sorted like before!)
        var maxPosition = DatasetSize * 10.0; // Large space to spread ranges
        
        for (var i = 0; i < DatasetSize; i++)
        {
            var start = random.NextDouble() * maxPosition;
            var length = 1.0 + random.NextDouble() * 5.0; // 1-6 units
            _sourceData.Add(new NumericRange<double, int>(start, start + length, i));
        }
    }

    /// <summary>
    /// Generates densely overlapping ranges in a smaller space.
    /// Creates worst-case scenario for pruning algorithms with high overlap.
    /// </summary>
    private void GenerateDenseData(Random random)
    {
        // Many overlapping ranges - worst case for pruning
        var maxPosition = DatasetSize * 2.0; // Smaller space = more overlap
        
        for (var i = 0; i < DatasetSize; i++)
        {
            var start = random.NextDouble() * maxPosition;
            var length = 5.0 + random.NextDouble() * 20.0; // Long ranges = more overlap
            _sourceData.Add(new NumericRange<double, int>(start, start + length, i));
        }
    }

    /// <summary>
    /// Generates sparsely distributed ranges with minimal overlap.
    /// Creates best-case scenario with ranges spread across large space.
    /// </summary>
    private void GenerateSparseData(Random random)
    {
        // Non-overlapping ranges - best case scenario
        var maxPosition = DatasetSize * 50.0; // Large space = minimal overlap
        
        for (var i = 0; i < DatasetSize; i++)
        {
            var start = random.NextDouble() * maxPosition;
            var length = 0.5 + random.NextDouble() * 1.0; // Short ranges = less overlap
            _sourceData.Add(new NumericRange<double, int>(start, start + length, i));
        }
    }

    /// <summary>
    /// Generates time-series like data with clustering patterns.
    /// Simulates real-world usage with localized range groupings.
    /// </summary>
    private void GenerateTemporalData(Random random)
    {
        // Time-series like data with clustering
        var clusters = Math.Max(10, DatasetSize / 1000); // Create clusters
        var rangesPerCluster = DatasetSize / clusters;
        
        for (var cluster = 0; cluster < clusters; cluster++)
        {
            var clusterCenter = cluster * 100.0 + random.NextDouble() * 20.0;
            
            for (var i = 0; i < rangesPerCluster && _sourceData.Count < DatasetSize; i++)
            {
                // Ranges clustered around center with some spread
                var start = clusterCenter + (random.NextDouble() - 0.5) * 30.0;
                var length = 1.0 + random.NextDouble() * 8.0;
                _sourceData.Add(new NumericRange<double, int>(start, start + length, _sourceData.Count));
            }
        }
    }

    /// <summary>
    /// Generates completely random ranges with varied lengths.
    /// Most realistic scenario for construction performance testing.
    /// </summary>
    private void GenerateRandomData(Random random)
    {
        // Completely random ranges - most realistic for construction benchmarks
        var maxPosition = DatasetSize * 20.0;
        
        for (var i = 0; i < DatasetSize; i++)
        {
            var start = random.NextDouble() * maxPosition;
            var length = 0.1 + random.NextDouble() * 10.0; // Varied lengths
            _sourceData.Add(new NumericRange<double, int>(start, start + length, i));
        }
    }

    /// <summary>
    /// Generates query ranges for performance testing.
    /// Uses different random seed to avoid correlation with source data.
    /// </summary>
    private void GenerateQueryRanges()
    {
        var random = new Random(RandomSeed + 1); // Different seed for queries
        _queryRanges = new List<NumericRange<double, object>>();

        for (var i = 0; i < QueryCount; i++)
        {
            var queryStart = random.NextDouble() * DatasetSize;
            var queryLength = 10.0 + random.NextDouble() * 50.0 * QueryLengthMultiplier;
            _queryRanges.Add(new NumericRange<double, object>(queryStart, queryStart + queryLength, new object()));
        }
    }

    /// <summary>
    /// Constructs both RangeFinder and IntervalTree data structures from source data.
    /// Both structures are built using the same source data for fair comparison.
    /// </summary>
    private void ConstructDataStructures()
    {
        // RangeFinder construction
        _rangeFinder = new RangeFinder<double, int>(_sourceData);

        // IntervalTree construction
        _intervalTree = new IntervalTree<double, NumericRange<double, int>>();
        foreach (var range in _sourceData)
        {
            _intervalTree.Add(range.Start, range.End, range);
        }
    }

    /// <summary>
    /// Executes all range queries against RangeFinder and returns results.
    /// Used by both benchmarks (for timing) and validation (for correctness).
    /// </summary>
    /// <returns>All results from range queries executed against RangeFinder</returns>
    protected List<NumericRange<double, int>> ExecuteRangeFinderRangeQueries()
    {
        var allResults = new List<NumericRange<double, int>>();
        foreach (var queryRange in _queryRanges)
        {
            allResults.AddRange(_rangeFinder.QueryRanges(queryRange.Start, queryRange.End));
        }
        return allResults;
    }

    /// <summary>
    /// Executes all point queries against RangeFinder and returns results.
    /// Uses midpoint of each query range as the point to search.
    /// </summary>
    /// <returns>All results from point queries executed against RangeFinder</returns>
    protected List<NumericRange<double, int>> ExecuteRangeFinderPointQueries()
    {
        var allResults = new List<NumericRange<double, int>>();
        foreach (var queryRange in _queryRanges)
        {
            var point = (queryRange.Start + queryRange.End) / 2.0; // Use midpoint
            allResults.AddRange(_rangeFinder.QueryRanges(point));
        }
        return allResults;
    }

    /// <summary>
    /// Executes all range queries against IntervalTree and returns results.
    /// Used by both benchmarks (for timing) and validation (for correctness).
    /// </summary>
    /// <returns>All results from range queries executed against IntervalTree</returns>
    protected List<NumericRange<double, int>> ExecuteIntervalTreeRangeQueries()
    {
        var allResults = new List<NumericRange<double, int>>();
        foreach (var queryRange in _queryRanges)
        {
            allResults.AddRange(_intervalTree.Query(queryRange.Start, queryRange.End));
        }
        return allResults;
    }

    /// <summary>
    /// Executes all point queries against IntervalTree and returns results.
    /// Uses midpoint of each query range as the point to search.
    /// </summary>
    /// <returns>All results from point queries executed against IntervalTree</returns>
    protected List<NumericRange<double, int>> ExecuteIntervalTreePointQueries()
    {
        var allResults = new List<NumericRange<double, int>>();
        foreach (var queryRange in _queryRanges)
        {
            var point = (queryRange.Start + queryRange.End) / 2.0; // Use midpoint
            allResults.AddRange(_intervalTree.Query(point));
        }
        return allResults;
    }

    /// <summary>
    /// Validates that RangeFinder and IntervalTree return identical results using the exact same methods as benchmarks
    /// </summary>
    private void ValidateResultCorrectness()
    {
        Console.WriteLine("🔍 Validating result correctness between RangeFinder and IntervalTree...");
        
        // 1. Validate range query results
        var rangeFinderResults = ExecuteRangeFinderRangeQueries();
        var intervalTreeResults = ExecuteIntervalTreeRangeQueries();
        ValidateResultSetsMatch(rangeFinderResults, intervalTreeResults, "Range queries");
        
        // 2. Validate point query results
        var rangeFinderPointResults = ExecuteRangeFinderPointQueries();
        var intervalTreePointResults = ExecuteIntervalTreePointQueries();
        ValidateResultSetsMatch(rangeFinderPointResults, intervalTreePointResults, "Point queries");
        
        Console.WriteLine($"✅ Validation passed! RangeFinder and IntervalTree return identical results.");
        Console.WriteLine($"   - Range queries: {rangeFinderResults.Count} total results");
        Console.WriteLine($"   - Point queries: {rangeFinderPointResults.Count} total results");
    }
    
    /// <summary>
    /// Validates that two result sets are identical using efficient HashSet comparison.
    /// Throws exception with detailed error information if results differ.
    /// </summary>
    private static void ValidateResultSetsMatch(List<NumericRange<double, int>> rangeFinderResults, 
        List<NumericRange<double, int>> intervalTreeResults, string queryType)
    {
        var rfSet = rangeFinderResults.ToHashSet();
        var itSet = intervalTreeResults.ToHashSet();
        
        if (rfSet.SetEquals(itSet)) return; // Fast path - sets are identical
        
        // Generate detailed error information
        Console.WriteLine($"❌ VALIDATION FAILED! {queryType} results differ between implementations:");
        Console.WriteLine($"   RangeFinder: {rangeFinderResults.Count} results");
        Console.WriteLine($"   IntervalTree: {intervalTreeResults.Count} results");
        
        var missing = itSet.Except(rfSet).Take(5);
        var extra = rfSet.Except(itSet).Take(5);
        
        if (missing.Any())
            Console.WriteLine($"   Missing from RangeFinder: {string.Join(", ", missing)}");
        if (extra.Any())
            Console.WriteLine($"   Extra in RangeFinder: {string.Join(", ", extra)}");
        
        throw new InvalidOperationException($"{queryType} validation failed - implementations return different result sets!");
    }
}