# RangeFinder.IO v0.2.0-preview

> **🧪 Experimental Pre-release**: RangeFinder.IO is an experimental package in early development. APIs may change significantly. Not recommended for production use.

File I/O and data generation utilities for the RangeFinder library.

## Features

- **CSV Serialization** - Import and export range data from CSV files
- **Parquet Support** - Binary serialization support
- **Data Generation** - Utilities for creating test datasets with various characteristics
- **Basic I/O Operations** - Fundamental file handling capabilities

## Quick Start

### Installation

```bash
dotnet add package RangeFinder.IO --prerelease
```

### CSV Import/Export

```csharp
using RangeFinder.IO.Serialization;

// Export ranges to CSV
var ranges = new[] {
    new NumericRange<double, string>(1.0, 2.0, "Range1"),
    new NumericRange<double, string>(3.0, 4.0, "Range2")
};
await RangeSerializerCsv.WriteCsvAsync("data.csv", ranges);

// Import ranges from CSV
var importedRanges = await RangeSerializerCsv.ReadCsvAsync<double, string>("data.csv");
var finder = RangeFinderFactory.Create(importedRanges);
```

### Parquet Support

```csharp
using RangeFinder.IO.Serialization;

// High-performance binary format
await RangeSerializerParquet.WriteParquetAsync("data.parquet", ranges);
var imported = await RangeSerializerParquet.ReadParquetAsync<double, string>("data.parquet");
```

### Data Generation

```csharp
using RangeFinder.IO.Generation;

// Generate test datasets
var generator = new Generator();
var testData = generator.GenerateRanges(
    count: 10000,
    characteristic: Characteristic.Dense,
    parameter: new Parameter { /* configuration */ }
);
```

## Requirements

- .NET 8.0 or later
- RangeFinder v0.2.0 (automatically included)

## Development Status

RangeFinder.IO is an experimental pre-release package. APIs are subject to significant changes. Use only for experimentation and testing. Not recommended for any production scenarios.

## Documentation

For complete documentation, examples, and integration guides, visit the [GitHub repository](https://github.com/dotnetduck/RangeFinder).

## License

MIT License - provided "as is" without warranty of any kind.
