# RangeFinder v0.2.0

A high-performance .NET range query library for general numeric ranges.

## What's New in v0.2.0

### New Features
- **RangeFinder.Visualizer** - Avalonia-based visualization tool for range data
- **RangeFinderFactory** - Simplified factory methods for RangeFinder construction from various data sources
- **Serialization Support** - CSV and Parquet serialization for range data via RangeFinder.IO

### Architecture Improvements
- **Modularization** - Migrated Generator functionality to separate RangeFinder.IO project
- **Enhanced Testing** - Property-based testing with validator migration
- **Test Organization** - Reorganized test structure into logical subdirectories

### Breaking Changes
- Generator functionality moved from Core to RangeFinder.IO project

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

## Documentation

For complete documentation, examples, and performance benchmarks, visit the [GitHub repository](https://github.com/dotnetduck/RangeFinder).

## License

MIT License