# RangeFinder Benchmark

## Overview

Performance benchmarks for RangeFinder using BenchmarkDotNet.

## Quick Start

```bash
# Build in release mode (required)
dotnet build -c Release

# Run single benchmark with parameters
dotnet run -c Release -- run-single --test <TestType> --characteristics <DatasetCharacteristic> \
  --accuracy <AccuracyLevel> [options]

# Run comprehensive suite with multiple characteristics
dotnet run -c Release -- run-suite --accuracy <AccuracyLevel> [options]
```

## Available Commands

### run-single

Run a single benchmark configuration:

- **Construction**: Data structure construction time measurement
- **RangeQuery**: Range query performance measurement
- **PointQuery**: Point query performance measurement
- **Allocation (Not yet implemented)**: Memory allocation pattern analysis during queries

### run-suite

Run comprehensive benchmark suite across multiple:

- Dataset sizes (default: 100K, 1M)
- Dataset characteristics (default: Uniform, Dense, Sparse)
- Test types (default: all types)

## Accuracy Levels

- **Quick**: Ultra-quick (0 warmup, 1 iteration) - ~5 minutes
- **Balanced (default)**: Balanced precision and speed - ~30 minutes  
- **Accurate**: Full statistical validation - ~2 hours

## Parameters

### Common Parameters

- **--accuracy**: Benchmark precision level (Quick, Balanced, Accurate) (default: Balanced)
- **--queries**: Number of queries to execute (default: 25)
- **--output**: Output directory for results (default: results/)
- **--timestamp**: Add timestamp to result files (default: true)

### run-single Parameters

- **--test**: Test type (Construction, RangeQuery, PointQuery, Allocation)
- **--size**: Dataset size (Size10K, Size100K, Size1M, Size5M) (default: Size100K)
- **--characteristics**: Dataset characteristic (Uniform, Dense, Sparse) (default: Uniform)
  - **Uniform**: Balanced distribution of ranges across the dataset space (baseline)
  - **Dense**: Many overlapping ranges in smaller space (worst-case for pruning algorithms)
  - **Sparse**: Minimal overlap with ranges spread across large space (best-case scenario)

### run-suite Parameters

- **--sizes**: Comma-separated sizes (default: "Size100K,Size1M")
- **--tests**: Comma-separated test types (default: all types)
- **--characteristics**: Dataset characteristics (default: "Uniform,Dense,Sparse")
- **--delay**: Seconds between benchmarks for system stabilization (default: 5)

## Examples

```bash
# Quick construction comparison (small dataset)
dotnet run -c Release -- run-single --test Construction --size Size10K --accuracy Quick

# Range query performance with specific parameters
dotnet run -c Release -- run-single --test RangeQuery --size Size100K --queries 50 --accuracy Balanced

# Memory allocation analysis
dotnet run -c Release -- run-single --test Allocation --accuracy Quick

# Complete benchmark suite with default characteristics
dotnet run -c Release -- run-suite --accuracy Balanced

# Suite with specific dataset characteristics
dotnet run -c Release -- run-suite --characteristics "Uniform,Dense" --sizes "Size100K" --accuracy Quick

# Comprehensive suite with all characteristics
dotnet run -c Release -- run-suite --characteristics "Uniform,Dense,Sparse" --accuracy Accurate
```

## Output Organization

Results are automatically organized in structured directories:

```text
results/
└── {timestamp}/        # Benchmark session timestamp
    ├── 10K/            # Dataset size
    │   ├── uniform/    # Dataset characteristic
    │   │   ├── construction_uniform_*.csv
    │   │   └── construction_uniform_*.md
    │   └── dense/      # Different characteristic
    └── 100K/
        └── uniform/
```

**File Types:**

- **CSV files**: Raw benchmark data for analysis
- **Markdown files**: Formatted BenchmarkDotNet reports
- **SUITE_SUMMARY.txt**: Overview of suite execution results

## Notes

- Always use Release mode for accurate performance measurement
- Results may vary based on hardware and dataset characteristics
- Construction benchmarks use properly randomized source data for fair sorting algorithm comparison
- Sequential execution prevents resource contention between benchmarks
- Static configuration parameters persist across benchmark runs within same process
