# RangeFinder.Core

High-performance range query library using binary search optimization.

For project overview and ecosystem information, see the [main README](../README.md).

## Features

- **O(log N + K) queries** - Fast overlap detection
- **RangeTree compatibility** - Drop-in replacement for existing code
- **Generic types** - Any `INumber<T>` (int, double, decimal, etc.)
- **Static optimization** - All ranges known upfront for maximum performance

## Quick Start

```csharp
using RangeFinder.Core;

// Create from tuples
var finder = RangeFinderFactory.Create([
    (1.0, 2.2, "Range1"),
    (2.0, 3.2, "Range2")
]);

// Query overlapping ranges
var values = finder.Query(2.0, 2.9);         // Returns: "Range1", "Range2"
var ranges = finder.QueryRanges(2.0, 3.0);   // Returns full range objects
```

## Custom Range Types

```csharp
public record TimeSlot(DateTime Start, DateTime End, string Activity) 
    : INumericRange<DateTime>;

var scheduler = new RangeFinder<DateTime, TimeSlot>(timeSlots);
```

## Advanced API

### Generic Constraints

- All numeric types implementing `INumber<T>` are supported
- Custom range types via `INumericRange<T>` interface

### Performance Characteristics  

- **Construction**: O(N log N) sorting + O(N) array setup
- **Query**: O(log N + K) where K = overlapping ranges  
- **Memory**: Efficient sorted array storage with cache locality

### RangeTree Migration

```csharp
// Before (RangeTree)
var tree = new IntervalTree<double, int>();
tree.Add(1.0, 5.0, 42);
var results = tree.Query(2.0, 4.0);

// After (RangeFinder) - same API
var finder = RangeFinderFactory.Create([(1.0, 5.0, 42)]);
var results = finder.Query(2.0, 4.0);
```
