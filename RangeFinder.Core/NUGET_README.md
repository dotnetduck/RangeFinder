# RangeFinder

A high-performance .NET range query library for general numeric ranges.
Designed as a lightweight alternative to RangeTree, optimized for scenarios where all ranges are known upfront.

**Part of the RangeFinder ecosystem** - Core library with additional packages for visualization, data generation, and analysis.

## What's New

- **🐛Fixed negative range handling** - Resolves incorrect query results when ranges contain negative values (does not affect positive-only ranges)
- **🏭Added RangeFinderFactory** - Simplified factory methods for RangeFinder construction from various data sources
- **🧪Enhanced Testing** - Property-based testing and improved coverage

## Features

- **High Performance**: Optimized binary search with fast construction and microsecond-level queries
- **Generic Support**: Works with any numeric type using `INumber<TSelf>` interface
- **Compatibility**: RangeTree-compatible Query APIs for easy migration

_Note: This Core package contains only the core range-finding algorithms. Additional packages for visualization and data I/O utilities are planned for future releases._

## Quick Start

```csharp
using RangeFinder.Core;

// Simple tuple-based creation
var finder = RangeFinderFactory.Create(
[
    (1.0, 2.2, 100),
    (2.0, 3.2, 200),
    (3.0, 4.0, 300)
]);

// Query overlapping ranges
var values = finder.Query(2.0, 2.9);         // Returns: 100, 200
var overlaps = finder.QueryRanges(2.0, 3.0); // Returns: [1.0,2.2]=100, [2.0,3.2]=200
```

## Development Status

RangeFinder is actively developed toward v1.0. The core API is stable and ready for evaluation, prototyping, and non-critical applications. For mission-critical systems, consider waiting for v1.0.0.

## RangeFinder Ecosystem

**Core** (this package)

- High-performance range queries with O(log N + K) complexity
- RangeTree API compatibility for easy migration  
- Generic support for any `INumber<T>` type

**Related Packages** (available on GitHub)

- **RangeFinder.IO** - Data generation and CSV/Parquet serialization
- **RangeFinder.Visualizer** - Interactive range visualization tool _(planned for NuGet)_

## Performance & Documentation

Our benchmarks show promising performance improvements over established libraries, with query times typically in the microsecond range for datasets up to 50M ranges.

For detailed benchmarks, complete documentation, and the full ecosystem, visit the [GitHub repository](https://github.com/dotnetduck/RangeFinder).
