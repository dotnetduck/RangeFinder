# RangeFinder

A high-performance .NET range query library for general numeric ranges.

[![.NET](https://img.shields.io/badge/.NET-8.0%20or%20later-blue)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)
[![nuget](https://img.shields.io/badge/nuget-v0.1.1-blue)](https://www.nuget.org/packages/RangeFinder/)

## Features

- **High Performance**: Fast construction, fast query.
- **Generic Support**: Works with any numeric type using [`INumber<TSelf>`](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.inumber-1) interface
- **Memory Efficient**: Predictable allocation patterns with low GC pressure (not yet measured)
- **Compatibility**: RangeTree-compatible [Query APIs](#migration-from-rangetree) for easy migration

## Quick Start

### Construct a RangeFinder

```csharp
using RangeFinder.Core;

// Simple tuple-based creation
var finder = RangeFinderFactory.Create(
[
    (1.0, 2.2, 100),
    (2.0, 3.2, 200),
    (3.0, 4.0, 300)
]);

// Or create from NumericRange objects
var ranges = new[]
{
    new NumericRange<double, int>(1.0, 2.2, 100),
    new NumericRange<double, int>(2.0, 3.2, 200)
};
var finder2 = RangeFinderFactory.Create(ranges);
```

### Issue queries

```csharp
// Query overlapping ranges
var values = finder.Query(2.0, 2.9);         // Returns: 100, 200
var overlaps = finder.QueryRanges(2.0, 3.0); // Returns: [1.0,2.2]=100, [2.0,3.2]=200

// Point queries
var pointValues = finder.Query(1.9);        // Returns: 100
var pointRanges = finder.QueryRanges(1.9);  // Returns: [1.0,2.2]=100
```

## Performance

RangeFinder is designed for high-performance range queries with optimized construction and query algorithms. 

Please refer a detailed performance analysis at
📊 **[Benchmark](RangeFinder.Benchmark)**

## API Reference

### Core Types

```csharp
// Basic range with associated value
var range = new NumericRange<double, int>(1.0, 5.0, 42);

// Range finder for efficient queries
var finder = new RangeFinder<double, int>(ranges);
```

### Query Methods

```csharp
// RangeTree-compatible API (returns values only)
IEnumerable<TValue> Query(TNumber point)
IEnumerable<TValue> Query(TNumber from, TNumber to)

// Native API (returns full range objects)
IEnumerable<NumericRange<TNumber, TValue>> QueryRanges(TNumber point)
IEnumerable<NumericRange<TNumber, TValue>> QueryRanges(TNumber from, TNumber to)
```

## Migration from RangeTree

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

## Requirements

- .NET 8.0 or later
- Supports any `INumber<T>` numeric type (int, double, decimal, etc.)

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions welcome! Please read our [contributing guidelines](CONTRIBUTING.md) and ensure all tests pass before submitting PRs.
