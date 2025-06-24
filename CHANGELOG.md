# Changelog

All notable changes to RangeFinder will be documented in this file.

## [Unreleased] - v0.3.0

### Added

#### RangeFinder.Core

- **IRangeFinder Interface** - Extracted interface from `RangeFinder` class for better abstraction and future extensibility
- **Interface-based Extension Methods** - Updated `IntervalTreeExtensions` to operate on interface types for improved flexibility

### Changed

#### RangeFinder.Core

- **RangeFinder Class** - Now implements `IRangeFinder<TNumber, TAssociated>` interface
- **Documentation Structure** - Moved XML documentation to interface definition as the authoritative contract location

#### RangeFinder.IO

- **Version Alignment** - Updated to 0.3.0-preview for consistency with v0.3.x development branch

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
