# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RangeFinder is a high-performance .NET 8.0 range query library optimized for fast overlap detection in numeric ranges. The library is designed for performance-critical scenarios with optimized algorithms and data structures.

## Solution Structure

- **RangeFinder.Core/** - Main library with Core interfaces, Finders implementations, and Utilities
- **RangeFinder.RangeTreeCompat/** - RangeTree compatibility layer for drop-in replacement
- **RangeFinderTests/** - NUnit test suite
- **RangeFinder.RangeTreeCompat.Tests/** - NUnit tests for compatibility wrapper
- **RangeFinder.Benchmark/** - BenchmarkDotNet performance tests
- **RangeFinder.Validator/** - Performance regression validation and compatibility testing

## Development Commands

```bash
# Build entire solution
dotnet build

# Run all unit tests
dotnet test

# Run specific test project
dotnet test RangeFinderTests/

# Run RangeTree compatibility wrapper tests
dotnet test RangeFinder.RangeTreeCompat.Tests/

# Run performance benchmarks
dotnet run --project RangeFinder.Benchmark -c Release

# Run performance guardian (regression detection)
dotnet run --project RangeFinder.Validator -c Release -- --guardian

# Test RangeTree wrapper compatibility
dotnet run --project RangeFinder.Validator -c Release -- wrapper

# Build in release mode for accurate performance testing
dotnet build -c Release
```

## Architecture

### Core Design
- **Primary Algorithm**: Binary search with intelligent pruning for O(log N + K) performance
- **Storage Strategy**: Sorted arrays for optimal cache locality
- **Type System**: Generic with `INumber<TNumber>` constraints for type safety

### Key Components
- **INumericRange<TNumber>**: Core range interface with overlap detection methods
- **IRangeFinder<TNumber, TCustomRange>**: Main finder interface
- **InMemoryRangeFinder**: Primary implementation using binary search + pruning
- **InMemoryRangeValueFinder**: Dictionary-based variant for key-value scenarios
- **BinarySearcher<T>**: Custom binary search utility with pruning optimization

### Performance Characteristics
The library is designed for performance-critical scenarios:
- Small to medium datasets (100K): 0.5-6μs per query depending on data pattern
- Large datasets (1M+): 0.6-6μs per query with consistent performance
- Construction performance: Optimized for fast initialization across all dataset sizes

## Testing Approach

### Unit Testing (NUnit)
- Uses NUnit 3.14.0 with standard SetUp/Test patterns
- Parametrized tests for multiple data types and scenarios
- Coverage includes overlap detection, custom range types, and edge cases

### Performance Testing (BenchmarkDotNet)
- Comprehensive benchmarks for performance validation
- Automated performance regression detection via PerformanceGuardian
- Strict performance baselines documented in PERFORMANCE_BASELINE.md

## Code Patterns

- **Generic Constraints**: Extensive use of `INumber<TNumber>` for numeric type safety
- **Immutable Design**: Preference for immutable collections where performance allows
- **Interface Segregation**: Clean separation between range definition and finding operations
- **Performance-First**: All design decisions prioritize query performance over convenience

## Performance Requirements

This codebase maintains strict performance requirements. Always run benchmarks after significant changes and ensure performance guardian tests pass before committing changes that could impact query performance.

## Critical Analysis Standards

**ALWAYS verify data before making claims.** The following standards must be followed when analyzing performance or benchmark results:

### Data Verification Requirements
1. **Read actual benchmark data first** - Never make claims without examining the raw numbers
2. **Apply sanity checks** - Question results that seem extraordinary or contradict basic logic
3. **Cross-reference multiple sources** - Compare BenchmarkDotNet results with manual measurements
4. **Use uncertain language** - Say "the data suggests..." not "this proves..." when evidence is limited

### Red Flags That Require Extra Scrutiny
- Performance claims >10x different from similar algorithms
- Memory usage claims >5x different when storing similar data
- Results that contradict multiple independent measurements
- Benchmark artifacts that seem too good to be true

### Presentation Standards
- **Never sound more confident than evidence warrants**
- **Explicitly flag uncertainty** when results don't make logical sense
- **Provide test conditions** with all performance claims for transparency
- **Distinguish between allocation patterns and actual memory footprint**

### Benchmark Design Standards
- **Use consistent baseline** - Maintain consistent baseline across benchmark classes
- **Consistent baseline across all benchmark classes** for comparable ratio calculations
- **Separate construction and query measurements** to isolate different performance aspects

### Historical Context
This standard was established after analysis errors led to false claims about memory efficiency advantages. Confident presentation of unverified data can mislead users into making incorrect technical decisions.

## RangeTree Compatibility Layer

### Overview
The `RangeFinder.RangeTreeCompat` project provides a drop-in replacement for the popular RangeTree library while leveraging RangeFinder's optimized performance. This compatibility layer allows existing RangeTree users to upgrade without code changes.

### Implementation Details
- **RangeTreeAdapter<TKey, TValue>**: Main wrapper class implementing `IIntervalTree<TKey, TValue>`
- **IntervalTree<TKey, TValue>**: Backward compatibility alias for existing code
- **Lazy Reconstruction**: Uses dirty flag pattern to rebuild internal `RangeFinder` only when needed
- **Generic Constraints**: Full support for `INumber<TKey>` numeric types

### API Compatibility
The wrapper provides 100% API compatibility with the original RangeTree:
```csharp
// Original RangeTree usage - works unchanged
var tree = new IntervalTree<int, string>();
tree.Add(1, 5, "A");
tree.Add(3, 7, "B");
var results = tree.Query(4); // Returns ["A", "B"]

// Preferred new naming for clarity
var adapter = new RangeTreeAdapter<int, string>();
adapter.Add(1, 5, "A");
var results = adapter.Query(4);
```

### Testing and Validation
- **26 Unit Tests**: Comprehensive coverage including edge cases and CRUD operations
- **Validator Integration**: Dynamic operation testing with cross-validation against RangeFinder
- **Bug Reproduction Tests**: Specific tests for edge cases found during validation
- **Performance Validation**: Maintains RangeFinder's O(log N + K) query performance

### Status: ✅ Complete
All compatibility wrapper functionality has been implemented and validated:
- ✅ Core CRUD operations (Add, Remove, Clear, Query)
- ✅ Point and range query support
- ✅ Bulk operations and duplicate value handling  
- ✅ Lazy reconstruction for optimal performance
- ✅ Full validator compatibility testing
- ✅ Comprehensive unit test coverage (26/26 passing)
- ✅ Integration with existing RangeFinder architecture