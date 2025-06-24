# Changelog

All notable changes to RangeFinder will be documented in this file.

## [0.2.0] - 2025-06-24

### Added

#### RangeFinder.Core

- **RangeFinderFactory** - Simplified factory methods for creating RangeFinder instances from tuples, collections, and various data sources
- **Enhanced Testing** - Property-based testing with FsCheck for comprehensive edge case coverage
- **Generic Type Safety** - Improved `INumber<T>` constraint handling for better compile-time safety

#### RangeFinder.IO

- **CSV Serialization** - Import and export range data in human-readable CSV format
- **Parquet Serialization** - Efficient binary format serialization for large datasets using Parquet.Net
- **Data Generation** - Configurable dataset generation with multiple distribution patterns (Uniform, Dense, Sparse, Temporal)
- **Statistical Analysis** - Dataset characteristic analysis including overlap density, range size distribution, and pattern validation
- **Parameterized Generation** - Flexible parameter system for controlling dataset characteristics and size

#### RangeFinder.Visualizer

- **Interactive Visualization** - Avalonia-based GUI tool for visualizing range datasets and query results
- **Multiple View Modes** - Support for different dataset patterns with real-time visualization updates
- **Sample Data** - Pre-built sample datasets demonstrating various use cases and patterns
- **Export Capabilities** - Save visualizations and generated datasets for further analysis

#### RangeFinder.Benchmark

- **Expanded Scale Support** - Benchmark support for datasets up to 50M ranges with automatic memory management
- **Multiple Data Patterns** - Comprehensive testing across Uniform, Dense, Sparse, and Temporal distributions
- **Automated Reporting** - Suite summary generation with organized result output by size and pattern

### Fixed

#### RangeFinder.Core

- **Negative Range Handling** - Fixed incorrect query results when ranges contain negative start or end values
- **Edge Case Handling** - Improved boundary condition handling for overlapping range detection
- **Memory Efficiency** - Optimized internal data structures for better cache locality

### Changed

#### RangeFinder.Benchmark

- **Default Test Configuration** - Allocation benchmarks temporarily disabled pending memory measurement accuracy improvements
- **Result Organization** - Improved benchmark result file organization with timestamp-based directory structure
- **Execution Strategy** - Sequential benchmark execution to avoid resource contention and ensure measurement accuracy

#### RangeFinder.Tests

- **Test Structure** - Reorganized test hierarchy into logical subdirectories for better maintainability
- **Coverage Expansion** - Added comprehensive property-based tests for edge cases and boundary conditions

## [0.1.0] - 2025-05-XX

### Added

- Initial release of RangeFinder.Core with basic range query functionality
- RangeTree API compatibility for easy migration from existing codebases
- Generic numeric type support using `INumber<T>` interface for type safety
- BenchmarkDotNet-based performance testing infrastructure
- Basic range overlap detection with O(log N + K) complexity
- Support for custom range types implementing `INumericRange<T>`
