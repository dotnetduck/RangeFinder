# RangeFinder.IO

> **Note**: This documentation was created with AI support and has not been fully reviewed by the author (dotnetduck).  
> It will be reviewed by v1.0.0. Types and documentation comments in the source code are more reliable at this moment.

Data generation and serialization utilities for testing and benchmarking.

## Features

- **Data patterns** - Uniform, Dense, Sparse distributions
- **File formats** - CSV (human-readable) and Parquet (efficient)
- **Statistical analysis** - Dataset characteristic validation

## Quick Start

### Generate Test Data

```csharp
using RangeFinder.IO.Generation;

// Generate 1000 ranges with dense overlapping pattern
var parameters = RangeParameterFactory.DenseOverlapping(1000);
var ranges = Generator.GenerateRanges<double>(parameters);

// Generate uniform distribution  
var uniformParams = RangeParameterFactory.Uniform(5000);
var uniformRanges = Generator.GenerateRanges<double>(uniformParams);
```

### Save/Load Data

```csharp
using RangeFinder.IO.Serialization;

// CSV format
var ranges = GenerateTestRanges();
await ranges.WriteCsvAsync("data.csv");
var loaded = await RangeSerializer.ReadCsvAsync<double, int>("data.csv");

// Parquet format (faster for large datasets)
await ranges.WriteParquetAsync("data.parquet");
var parquetLoaded = await RangeSerializer.ReadParquetAsync<double, int>("data.parquet");
```

### Load into RangeFinder

```csharp
using RangeFinder.IO.Serialization;

// Create RangeFinder directly from files
var finder = await RangeFinderLoader.FromCsvAsync<double, int>("data.csv");
var parquetFinder = await RangeFinderLoader.FromParquetAsync<double, int>("data.parquet");

// Use the finder
var results = finder.Query(1.0, 5.0);
```

## Data Patterns

- **Uniform** - Evenly distributed, moderate overlap
- **Dense** - High overlap, stress-test scenarios  
- **Sparse** - Minimal overlap, optimal performance
- **Clustered** - Grouped ranges with gaps between clusters

## API Reference

### Serialization

Static methods in `RangeSerializer`:

```csharp
// CSV operations
IEnumerable<NumericRange<TNumber, TAssociated>> ReadCsv<TNumber, TAssociated>(string filePath)
Task<IEnumerable<NumericRange<TNumber, TAssociated>>> ReadCsvAsync<TNumber, TAssociated>(string filePath)

// Parquet operations  
IEnumerable<NumericRange<TNumber, TAssociated>> ReadParquet<TNumber, TAssociated>(string filePath)
Task<IEnumerable<NumericRange<TNumber, TAssociated>>> ReadParquetAsync<TNumber, TAssociated>(string filePath)

// Extension methods for writing
ranges.WriteCsv("file.csv")
ranges.WriteParquet("file.parquet")
await ranges.WriteCsvAsync("file.csv")
await ranges.WriteParquetAsync("file.parquet")
```

### Generation

```csharp
// Factory methods for common patterns
var denseParams = RangeParameterFactory.DenseOverlapping(1000);
var sparseParams = RangeParameterFactory.SparseNonOverlapping(1000);
var uniformParams = RangeParameterFactory.Uniform(1000);
var clusteredParams = RangeParameterFactory.Clustered(1000);

// Generate ranges
var ranges = Generator.GenerateRanges<double>(parameters);

// Custom parameters
var customParams = new Parameter
{
    Count = 1000,
    SpacePerRange = 5.0,
    LengthRatio = 1.2,
    LengthVariability = 0.5,
    OverlapFactor = 2.0,
    ClusteringFactor = 0.3,
    RandomSeed = 42
};
```

### Loading

```csharp
// Create RangeFinder from files
RangeFinder<TNumber, TAssociated> FromCsv<TNumber, TAssociated>(string filePath)
Task<RangeFinder<TNumber, TAssociated>> FromCsvAsync<TNumber, TAssociated>(string filePath)
RangeFinder<TNumber, TAssociated> FromParquet<TNumber, TAssociated>(string filePath)
Task<RangeFinder<TNumber, TAssociated>> FromParquetAsync<TNumber, TAssociated>(string filePath)
```
