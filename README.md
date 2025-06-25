# RangeFinder

A high-performance .NET range query library for general numeric ranges. Designed as a lightweight alternative to
RangeTree, optimized for scenarios where all ranges are known upfront.

[![.NET](https://img.shields.io/badge/.NET-8.0%20or%20later-blue)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)
[![nuget](https://img.shields.io/badge/nuget-v0.2.0-blue)](https://www.nuget.org/packages/RangeFinder/)

## Features

- **High Performance**: Fast construction, fast query
- **Generic Support**: Works with any numeric type using
  [`INumber<TSelf>`](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.inumber-1) interface
- **Compatibility**: RangeTree-compatible [Query APIs](#rangetree-compatible-query-apis) for easy migration
- **Visualization**: Interactive range visualization with RangeFinder.Visualizer (Experimental)
- **Serialization**: CSV and Parquet data import/export support (Experimental)

## Quick Start

```csharp
using RangeFinder.Core;

// Create from tuples  
var finder = RangeFinderFactory.Create([(1.0, 2.2, 100), (2.0, 3.2, 200)]);

// Query overlapping ranges
var values = finder.Query(2.0, 2.9);         // Returns: 100, 200
var ranges = finder.QueryRanges(2.0, 3.0);   // Returns full range objects
```

For detailed API documentation and advanced usage examples, see [RangeFinder.Core](src/RangeFinder.Core/#quick-start).

## Performance Characteristics (Preliminary)

The following results are preliminary measurements from our test environment and may not reflect typical
real-world performance differences. These should be validated with your specific use cases.

**Test Environment:**

- RangeFinder v0.2.0
- RangeTree (IntervalTree) v3.0.1 (for comparison)

### Range Query Performance

| Dataset | Pattern | RangeFinder | IntervalTree | Performance Ratio |
|---------|---------|------------:|-------------:|-------------------|
| 10M | Uniform | 2.16 us | 17.89 us | **~8x improvement** |
| 10M | Dense | 6.66 us | 33.43 us | **~5x improvement** |
| 10M | Sparse | 1.27 us | 12.15 us | **~10x improvement** |
| 50M | Uniform | 2.42 us | 20.31 us | **~8x improvement** |

### Point Query Performance

| Dataset | Pattern | RangeFinder | IntervalTree | Performance Ratio |
|---------|---------|------------:|-------------:|-------------------|
| 10M | Uniform | 993 ns | 9,272 ns | **~9x improvement** |

### Construction Performance

| Dataset | Pattern | RangeFinder | IntervalTree | Performance Ratio |
|---------|---------|------------:|-------------:|-------------------|
| 5M | Uniform | 834 ms | 10,375 ms | **~12x improvement** |
| 10M | Uniform | 1,888 ms | 26,999 ms | **~14x improvement** |
| 50M | Uniform | 11.30 s | 207.41 s | **~18x improvement** |

## Methodology Notes

All benchmarks executed using BenchmarkDotNet with:

- Release mode compilation
- Sequential execution for measurement accuracy
- Multiple iterations for statistical reliability
- Platform: ARM64 macOS with .NET 8.0

_These are preliminary benchmark results from our specific test environment. Performance differences may not
reflect real-world usage and can vary significantly on different platforms, with different data patterns, or under
different usage conditions. We strongly encourage users to conduct their own benchmarks with their specific data
and requirements before making performance-based decisions._

_For detailed methodology, benchmarking tools, and to provide feedback on our approach, see [RangeFinder.Benchmark/README.md](test/RangeFinder.Benchmark/README.md)._

## RangeTree-compatible query APIs

RangeFinder provides query API compatibility for easy migration:

```csharp
// Before
var tree = new IntervalTree<double, int>();
tree.Add(1.0, 5.0, 42);
var results = tree.Query(2.0, 4.0);

// After  
var finder = RangeFinderFactory.Create(new[] { (1.0, 5.0, 42) });
var results = finder.Query(2.0, 4.0); // Same API!
```

**Key Differences:**

- **Dynamic insertion**: Not supported - requires all ranges during construction
- **Query API**: Compatible method signatures and behavior
- **Performance focus**: Optimized for fast construction and queries

## Architecture

### Core Projects

- **[RangeFinder.Core](src/RangeFinder.Core/)** - Main library with range finding algorithms
- **[RangeFinder.IO](src/RangeFinder.IO/)** - File I/O and data generation utilities
- **[RangeFinder.Visualizer](src/RangeFinder.Visualizer/)** - Avalonia-based range visualization tool
- **[RangeFinder.Benchmark](test/RangeFinder.Benchmark/)** - Performance testing and validation

## What's New

See [CHANGELOG.md](CHANGELOG.md) for detailed version history and changes across all projects.

## Requirements

- .NET 8.0 or later
- Supports any `INumber<T>` numeric type (int, double, decimal, etc.)

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions welcome! Please read our [contributing guidelines](CONTRIBUTING.md) and ensure all tests pass before submitting PRs.
