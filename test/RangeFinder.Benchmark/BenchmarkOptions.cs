namespace RangeFinder.Benchmarks;

/// <summary>
/// Type of benchmark to execute
/// </summary>
public enum TestType
{
    /// <summary>Data structure construction time comparison</summary>
    Construction,

    /// <summary>Range query execution performance comparison</summary>
    RangeQuery,

    /// <summary>Point query execution performance comparison</summary>
    PointQuery,

    /// <summary>Memory allocation during queries analysis (temporarily disabled - see TODO)</summary>
    Allocation,

    /// <summary>Run all core benchmark types</summary>
    All
}

/// <summary>
/// Benchmark precision and duration level
/// </summary>
public enum AccuracyLevel
{
    /// <summary>Ultra-quick benchmark (~30 seconds, minimal iterations)</summary>
    Quick,

    /// <summary>Balanced benchmark with reasonable precision (~2 minutes, recommended)</summary>
    Balanced,

    /// <summary>Comprehensive benchmark with full statistical validation (~5-15 minutes)</summary>
    Accurate
}

/// <summary>
/// Dataset characteristic for testing different scenarios
/// </summary>
public enum DatasetCharacteristic
{
    /// <summary>Uniform distribution (baseline, balanced scenario)</summary>
    Uniform,

    /// <summary>Dense overlapping ranges (worst case for pruning algorithms)</summary>
    Dense,

    /// <summary>Sparse non-overlapping ranges (best case scenario)</summary>
    Sparse,

    /// <summary>Time-series patterns (real-world usage scenarios)</summary>
    Temporal,

    /// <summary>Test all dataset characteristics</summary>
    All
}

/// <summary>
/// Dataset size for testing scalability
/// </summary>
public enum DatasetSize
{
    /// <summary>Small dataset: 10,000 elements</summary>
    Size10K,

    /// <summary>Medium dataset: 100,000 elements (common baseline)</summary>
    Size100K,

    /// <summary>Large dataset: 1,000,000 elements</summary>
    Size1M,

    /// <summary>Very large dataset: 5,000,000 elements</summary>
    Size5M,

    /// <summary>Extra large dataset: 10,000,000 elements</summary>
    Size10M,

    /// <summary>Massive dataset: 50,000,000 elements</summary>
    Size50M,

    /// <summary>Test all dataset sizes</summary>
    All
}

/// <summary>
/// Extension methods for enum conversions and utilities
/// </summary>
public static class BenchmarkOptionsExtensions
{
    public static int ToElementCount(this DatasetSize size) => size switch
    {
        DatasetSize.Size10K => 10_000,
        DatasetSize.Size100K => 100_000,
        DatasetSize.Size1M => 1_000_000,
        DatasetSize.Size5M => 5_000_000,
        DatasetSize.Size10M => 10_000_000,
        DatasetSize.Size50M => 50_000_000,
        _ => throw new ArgumentException($"Cannot convert {size} to element count")
    };

    public static string ToDisplayString(this DatasetSize size) => size switch
    {
        DatasetSize.Size10K => "10K",
        DatasetSize.Size100K => "100K",
        DatasetSize.Size1M => "1M",
        DatasetSize.Size5M => "5M",
        DatasetSize.Size10M => "10M",
        DatasetSize.Size50M => "50M",
        DatasetSize.All => "all",
        _ => size.ToString()
    };

    public static string ToDisplayString(this TestType test) => test switch
    {
        TestType.Construction => "construction",
        TestType.RangeQuery => "range-query",
        TestType.PointQuery => "point-query",
        TestType.Allocation => "allocation",
        TestType.All => "all",
        _ => test.ToString().ToLower()
    };

    public static string ToDisplayString(this AccuracyLevel accuracy) => accuracy switch
    {
        AccuracyLevel.Quick => "quick",
        AccuracyLevel.Balanced => "balanced",
        AccuracyLevel.Accurate => "accurate",
        _ => accuracy.ToString().ToLower()
    };

    public static string ToDisplayString(this DatasetCharacteristic dataset) => dataset switch
    {
        DatasetCharacteristic.Uniform => "uniform",
        DatasetCharacteristic.Dense => "dense",
        DatasetCharacteristic.Sparse => "sparse",
        DatasetCharacteristic.Temporal => "temporal",
        DatasetCharacteristic.All => "all",
        _ => dataset.ToString().ToLower()
    };
}
