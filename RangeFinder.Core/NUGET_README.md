# RangeFinder v0.2.0

A high-performance .NET range query library for general numeric ranges.

## What's New in v0.2.0

### 🔧 Bug Fixes
- **Fixed negative range handling** - Resolves incorrect query results when ranges contain negative values (does not affect positive-only ranges)

### ✨ New Features  
- **RangeFinderFactory** - Simplified factory methods for RangeFinder construction from various data sources

## Features

- **High Performance**: Fast construction, fast query
- **Generic Support**: Works with any numeric type using `INumber<TSelf>` interface
- **Memory Efficient**: Predictable allocation patterns with low GC pressure
- **Compatibility**: RangeTree-compatible Query APIs for easy migration

## Quick Start

### Installation

```bash
dotnet add package RangeFinder
```

### Basic Usage

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

## Requirements

- .NET 8.0 or later
- Supports any `INumber<T>` numeric type (int, double, decimal, etc.)

## Development Status

RangeFinder is actively developed toward v1.0. The core API is stable and ready for evaluation, prototyping, and non-critical applications. For mission-critical systems, consider waiting for v1.0.0.

## Documentation

For complete documentation, examples, and performance benchmarks, visit the [GitHub repository](https://github.com/dotnetduck/RangeFinder).

## License

MIT License - provided "as is" without warranty of any kind.