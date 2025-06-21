# RangeFinder

A high-performance .NET range query library for general numeric ranges. Inspired by RangeTree library with focus on performance optimization.

## Features

- **High Performance**: Fast construction, fast query
- **Generic Support**: Works with any numeric type (`int`, `double`, `decimal`, etc.)
- **Simple API**: Easy-to-use interface for range overlap detection
- **Memory Efficient**: Optimized for cache locality and minimal allocations
- **Thread Safe**: Immutable data structures for concurrent access

## Quick Start

```csharp
using RangeFinder.Core;

// Create ranges
var ranges = new List<NumericRange<double, string>>
{
    new(1.0, 5.0, "Range1"), new(3.0, 7.0, "Range2"), new(10.0, 15.0, "Range3")
};

// Build range finder
var finder = RangeFinderFactory.Create(ranges);

// Query overlapping ranges
var overlaps = finder.QueryRanges(4.0, 6.0); // Returns: Range1, Range2
```

For detailed documentation and examples, visit the [GitHub repository](https://github.com/dotnetduck/RangeFinder).