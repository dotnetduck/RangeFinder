# Changelog

All notable changes to RangeFinder will be documented in this file.

## [0.2.0] - 2025-06-24

### Added

#### RangeFinder.Core

- **RangeFinderFactory** - Simplified factory methods for creating RangeFinder instances from tuples, collections, and various data sources
- **Enhanced Testing** - Property-based testing with FsCheck for comprehensive edge case coverage

#### RangeFinder.IO (Experimental)

- **CSV Serialization** - Import and export range data in human-readable CSV format
- **Parquet Serialization** - Efficient binary format serialization for large datasets using Parquet.Net
- **Data Generation** - Configurable dataset generation with multiple distribution patterns (Uniform, Dense, Sparse, Temporal)
- **Statistical Analysis** - Dataset characteristic analysis including overlap density, range size distribution, and pattern validation
- **Parameterized Generation** - Flexible parameter system for controlling dataset characteristics and size

#### RangeFinder.Visualizer (Experimental)

- **Interactive Visualization** - Avalonia-based GUI tool for visualizing range datasets and query results

#### RangeFinder.Benchmark

- **Multiple Data Patterns** - Comprehensive testing across Uniform, Dense, Sparse, and Temporal distributions
- **Automated Reporting** - Suite summary generation with organized result output by size and pattern

### Fixed

#### RangeFinder.Core

- **Negative Range Handling** - Fixed incorrect query results when ranges contain negative start or end values

### Changed

#### RangeFinder.Tests

- **Test Structure** - Reorganized test hierarchy into logical subdirectories for better maintainability
- **Coverage Expansion** - Added comprehensive property-based tests for edge cases and boundary conditions

## [0.1.0] - 2025-06-15

### Initial release🎉
