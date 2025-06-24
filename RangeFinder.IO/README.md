# RangeFinder.IO

Data generation and serialization utilities for testing and benchmarking.

## Features

- **Data patterns** - Uniform, Dense, Sparse, Temporal distributions
- **File formats** - CSV (human-readable) and Parquet (efficient)
- **Statistical analysis** - Dataset characteristic validation

## Quick Start

### Generate Test Data

```csharp
using RangeFinder.IO.Generation;

var data = Generator.GenerateDataset<double, int>(
    size: 10_000,
    characteristic: Characteristic.Uniform
);
```

### Save/Load Data

```csharp
using RangeFinder.IO.Serialization;

// CSV format
var csv = new RangeSerializerCsv<double, int>();
await csv.SerializeAsync(ranges, "data.csv");
var loaded = await csv.DeserializeAsync("data.csv");

// Parquet format (faster for large datasets)
var parquet = new RangeSerializerParquet<double, int>();
await parquet.SerializeAsync(ranges, "data.parquet");
```

## Data Patterns

- **Uniform** - Evenly distributed, moderate overlap
- **Dense** - High overlap, stress-test scenarios  
- **Sparse** - Minimal overlap, optimal performance
- **Temporal** - Time-series patterns, realistic overlap
