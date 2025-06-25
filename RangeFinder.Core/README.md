# RangeFinder.Core

> **Note**: This documentation was created with AI support and has not been fully reviewed by the author (dotnetduck).  
> It will be reviewed by v1.0.0. Types and documentation comments in the source code are more reliable at this moment.

High-performance range query library using binary search optimization.

For project overview and ecosystem information, see the [main README](../README.md).

## Quick Start

```csharp
using RangeFinder.Core;

// Create from tuples
var finder = RangeFinderFactory.Create([
    (1.0, 2.2, "Range1"),
    (2.0, 3.2, "Range2")
]);

// Query overlapping values (using extension method)
var values = finder.Query(2.0, 2.9);         // Returns: "Range1", "Range2"

// Query overlapping ranges (native method)
var ranges = finder.QueryRanges(2.0, 3.0);   // Returns full range objects
```

## API Overview

RangeFinder provides two query methods:

### `Query()` - Extension Method

Returns associated values directly (IntervalTree compatible):

```csharp
IEnumerable<TAssociated> Query(TNumber from, TNumber to)
IEnumerable<TAssociated> Query(TNumber point)
```

### `QueryRanges()` - Native Method

Returns full `NumericRange<TNumber, TAssociated>` objects:

```csharp
IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber from, TNumber to)
IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber point)
```

## RangeTree Migration

RangeFinder provides a drop-in replacement for IntervalTree/RangeTree:

```csharp
// Before (RangeTree)
var tree = new IntervalTree<double, int>();
tree.Add(1.0, 5.0, 42);
var results = tree.Query(2.0, 4.0);

// After (RangeFinder) - same API
var finder = RangeFinderFactory.Create([(1.0, 5.0, 42)]);
var results = finder.Query(2.0, 4.0);
```

## Performance Characteristics

- **Algorithm**: Binary search with intelligent pruning
- **Query Time**: O(log N + K) where K is result count
- **Construction**: Optimized for fast initialization
- **Memory**: Sorted arrays for optimal cache locality

## Factory Methods

```csharp
// From tuples
RangeFinderFactory.Create([(1.0, 2.0, "A"), (2.0, 3.0, "B")])

// From NumericRange objects
RangeFinderFactory.Create(numericRanges)

// From arrays
RangeFinderFactory.Create(starts, ends, values)

// Without values (uses indices)
RangeFinderFactory.Create([(1.0, 2.0), (2.0, 3.0)])

// Empty instance
RangeFinderFactory.CreateEmpty<double, string>()
```
