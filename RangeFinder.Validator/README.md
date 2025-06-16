# RangeFinder Validator

A comprehensive validation tool for testing RangeFinder correctness and RangeTree compatibility wrapper performance.

## Overview

The validator provides two main testing modes:
- **Core RangeFinder validation**: Correctness testing against reference implementations
- **RangeTree compatibility testing**: Validates the IntervalTree wrapper with performance comparisons

Results vary as datasets and queries are randomly generated for thorough testing.

## Commands

### Core RangeFinder Validation
```bash
# Run single correctness test
dotnet run -- single [--characteristic Uniform] [--size 25000] [--queries 500]

# Run continuous testing until failure
dotnet run -- continuous [--maxTests 0] [--reportInterval 10]

# Run validation suite across characteristics
dotnet run -- validate [--sizes "10000,25000,50000"] [--queries 500]
```

### RangeTree Compatibility Testing
```bash
# Test wrapper with dynamic operations
dotnet run -- wrapper [--characteristic Uniform] [--size 25000] [--operations 200]

# Run continuous wrapper testing with performance metrics
dotnet run -- wrapper-continuous [--maxTests 0] [--reportInterval 10]
```

## Dataset Characteristics

- **Uniform**: Evenly distributed ranges
- **DenseOverlapping**: High overlap density
- **SparseNonOverlapping**: Minimal overlaps
- **Clustered**: Grouped range patterns

## Performance Comparison

The wrapper testing includes fair performance comparison where both implementations pay equivalent reconstruction costs for dynamic operations (Add/Remove), typically showing RangeFinder ~4x faster than the compatibility wrapper.