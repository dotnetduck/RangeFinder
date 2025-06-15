# RangeGenerator

A .NET library for generating parameterized range datasets with controlled statistical properties for systematic performance testing and benchmarking of range query algorithms.

## Overview

RangeGenerator provides sophisticated tools for creating test datasets with precise statistical characteristics, enabling systematic validation of range query performance across diverse data patterns. It can be used with any range-based data structure or algorithm.

## Features

- **Parameterized Generation**: Control range distribution with statistical parameters
- **Predefined Presets**: Common test scenarios (dense overlapping, sparse, time-series, etc.)
- **Multiple Data Types**: Support for any `INumber<T>` type (int, double, float, etc.)
- **Reproducible Results**: Deterministic generation with configurable seeds
- **Statistical Analysis**: Built-in dataset analysis and validation
- **Query Generation**: Generate corresponding query ranges and points for testing

## Quick Start

### Basic Usage

```csharp
using RangeFinder.Generator;
using RangeFinder.Core;

// Generate 1000 ranges with time-series characteristics
var parameters = RangeParameterFactory.TimeSeries(1000);
var ranges = Generator.GenerateRanges<double>(parameters);

// Generate query ranges for testing
var queryRanges = Generator.GenerateQueryRanges<double>(parameters, 100);

// Generate point queries
var queryPoints = Generator.GenerateQueryPoints<double>(parameters, 50);
```

### Predefined Presets

```csharp
// Dense overlapping - stress testing with high overlap
var dense = RangeParameterFactory.DenseOverlapping(1000);

// Sparse non-overlapping - best-case performance testing  
var sparse = RangeParameterFactory.SparseNonOverlapping(1000);

// Time series - realistic temporal data simulation
var timeSeries = RangeParameterFactory.TimeSeries(1000);

// Clustered - grouped data with gaps between clusters
var clustered = RangeParameterFactory.Clustered(1000);

// Uniform - balanced baseline testing
var uniform = RangeParameterFactory.Uniform(1000);
```

### Custom Parameters

```csharp
var customParams = RangeParameterFactory.Custom(
    count: 5000,
    spacePerRange: 10.0,
    lengthRatio: 1.0,
    overlapFactor: 2.0,
    lengthVariability: 0.2,
    clusteringFactor: 0.3
) with { RandomSeed = 42 };

var ranges = Generator.GenerateRanges<int>(customParams);
```

### Dataset Analysis

```csharp
var analysis = Analyzer.Analyze(ranges);
Console.WriteLine($"Count: {analysis.Count}");
Console.WriteLine($"Range: [{analysis.MinValue:F2}, {analysis.MaxValue:F2}]");
Console.WriteLine($"Avg Length: {analysis.AverageLength:F2} ± {analysis.LengthStdDev:F2}");
Console.WriteLine($"Overlap %: {analysis.OverlapPercentage:F1}%");
```

## Integration Examples

### With RangeFinder

```csharp
var parameters = RangeParameterFactory.TimeSeries(10_000);
var ranges = Generator.GenerateRanges<double>(parameters);
var queryRanges = Generator.GenerateQueryRanges<double>(parameters, 1000);

var rangeFinder = new InMemoryRangeValueFinder<double, int>(ranges);

// Benchmark query performance
foreach (var query in queryRanges)
{
    var results = rangeFinder.Query(query.Start, query.End);
    // Process results...
}
```

### With Other Libraries

```csharp
// Example with IntervalTree
var ranges = Generator.GenerateRanges<double>(parameters);
var intervalTree = new IntervalTree<double, int>();

foreach (var range in ranges)
{
    intervalTree.Add(range.Start, range.End, range.Value);
}
```

## Preset Characteristics

| Preset | Overlap Rate | Use Case |
|--------|-------------|----------|
| **Dense Overlapping** | ~200% | Stress testing, worst-case performance with heavy overlap |
| **Sparse Non-Overlapping** | ~25% | Lower overlap, better performance testing |
| **Time Series** | ~45% | Realistic temporal data simulation |
| **Clustered** | ~110% | Non-uniform data distribution with cluster overlap |
| **Uniform** | ~57% | General-purpose validation, standard benchmarks |

## API Reference

### Core Classes

#### Generator
Static methods for range and query generation:
- `GenerateRanges<TNumber>(Parameter parameters)` - Generate ranges with specified characteristics
- `GenerateQueryRanges<TNumber>(Parameter parameters, int queryCount, double multiplier = 2.0)` - Generate query ranges for testing
- `GenerateQueryPoints<TNumber>(Parameter parameters, int queryCount)` - Generate point queries for testing

#### Analyzer
Static methods for dataset analysis:
- `Analyze<TNumber>(IEnumerable<NumericRange<TNumber, int>> ranges)` - Analyze dataset characteristics and return Stats

#### Parameter
Configuration record for controlling range generation characteristics:

```csharp
public record Parameter
{
    public int Count { get; init; }                    // Number of ranges to generate
    public double SpacePerRange { get; init; }         // Average space per range
    public double LengthRatio { get; init; }           // Ratio of range length to space
    public double LengthVariability { get; init; }     // Length variation coefficient
    public double OverlapFactor { get; init; }         // Target overlap depth
    public double ClusteringFactor { get; init; }      // Clustering tendency
    public double StartOffset { get; init; } = 0.0;    // Start offset in total space
    public int RandomSeed { get; init; } = 42;         // Seed for reproducible results

    // Calculated properties
    public double TotalSpace => Count * SpacePerRange;
    public double AverageLength => SpacePerRange * LengthRatio;
}
```

#### Stats
Analysis results record containing dataset statistics:
- Count, range bounds, average length, standard deviations, overlap percentage

#### Characteristic
Enum defining available preset types:
- `DenseOverlapping`, `SparseNonOverlapping`, `TimeSeries`, `Clustered`, `Uniform`

### Factory Methods

The `RangeParameterFactory` class provides convenient preset creation:
- `DenseOverlapping(int count)` - High overlap stress testing
- `SparseNonOverlapping(int count)` - Zero overlap baseline testing  
- `TimeSeries(int count)` - Realistic temporal patterns
- `Clustered(int count)` - Grouped data with clustering
- `Uniform(int count)` - Balanced general-purpose testing
- `Custom(...)` - Full parameter control

## Advanced Usage

### Reproducible Testing

```csharp
// Same seed produces identical results
var params1 = RangeParameterFactory.Custom(/* ... */) with { RandomSeed = 42 };
var params2 = RangeParameterFactory.Custom(/* ... */) with { RandomSeed = 42 };

var ranges1 = Generator.GenerateRanges<double>(params1);
var ranges2 = Generator.GenerateRanges<double>(params2);
// ranges1 and ranges2 are identical
```

### Multiple Numeric Types

```csharp
// Integer ranges
var intRanges = Generator.GenerateRanges<int>(parameters);

// Float ranges  
var floatRanges = Generator.GenerateRanges<float>(parameters);

// Decimal ranges
var decimalRanges = Generator.GenerateRanges<decimal>(parameters);
```

### Parameter Validation

```csharp
var parameters = RangeParameterFactory.Custom(/* ... */);
try 
{
    parameters.Validate(); // Throws ArgumentException if invalid
    var ranges = Generator.GenerateRanges<double>(parameters);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid parameters: {ex.Message}");
}
```

## Statistical Properties

### Range Length Distribution
Ranges are generated using normal distribution with configurable mean and standard deviation, ensuring realistic length variations.

### Range Interval Distribution
Spacing between range starts follows normal distribution, controlling overlap characteristics and data density.

### Overlap Control
The relationship between `LengthRatio` and `OverlapFactor` determines overlap probability:
- **High Overlap**: Large ranges with small intervals
- **Low Overlap**: Small ranges with large intervals  
- **Clustered**: High interval variance creates natural clustering

## Performance Considerations

- **Generation Time**: O(N) where N is the count parameter
- **Memory Usage**: O(N) for storing generated ranges
- **Deterministic**: Same parameters always produce identical results
- **Thread Safety**: All methods are thread-safe (no shared state)

## Dependencies

- **.NET 8.0**: Target framework
- **RangeFinder.Core**: For `NumericRange<TNumber, TValue>` types
- **System.Numerics**: For `INumber<T>` constraint support

## Related Projects

- **RangeFinder**: High-performance range query library this generator was designed to test
- **RangeGeneratorTests**: Comprehensive test suite for the generator functionality

## Contributing

This library is part of the RangeFinder project ecosystem. When contributing:

1. Maintain deterministic behavior for reproducible testing
2. Follow existing statistical distribution patterns
3. Add comprehensive tests for new presets or functionality
4. Update documentation for API changes

## License

[Include appropriate license information]