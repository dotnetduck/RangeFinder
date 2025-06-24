# RangeFinder Test Suite

> **Note**: This documentation was created with AI support and has not been fully reviewed by the author (dotnetduck).  
> It will be reviewed by v1.0.0. Types and documentation comments in the source code are more reliable at this moment.

Comprehensive test suite for the RangeFinder library, ensuring correctness, performance, and reliability across all components.

## Test Organization

The test suite is organized into focused modules, each testing specific aspects of the library:

### Core Tests (5 files)

| File | Tests | Purpose |
|------|-------|---------|
| `RangeFinderTests.cs` | 14 tests | Core functionality testing with parameterized scenarios |
| `RangeTreeCompatibilityTests.cs` | 10 tests | Compatibility validation against IntervalTree |
| `RangeFinderEdgeCaseTests.cs` | 3 tests | Edge cases and boundary conditions |
| `RangeFinderFactoryTests.cs` | 22 tests | Factory method validation and error handling |
| `RangeFinderGenericTypeTests.cs` | 5 tests | Generic type system validation |

**Total Core Tests: 54**

### Generation Tests (6 files)

| File | Tests | Purpose |
|------|-------|---------|
| `DatasetGenerationTests.cs` | 7 tests | Test data generation utilities |
| `CharacteristicTests.cs` | 8 tests | Dataset characteristic validation |
| `ParameterValidationTests.cs` | 9 tests | Parameter validation and boundary testing |
| `ParameterizedDatasetTests.cs` | 4 TestCases | Parameterized dataset validation |
| `QueryGenerationTests.cs` | 6 tests | Query generation utilities |
| `DatasetAnalysisTests.cs` | 11 tests | Statistical analysis of datasets |

**Total Generation Tests: 45**

### Serialization Tests (3 files)

| File | Tests | Purpose |
|------|-------|---------|
| `RangeSerializerCsvTests.cs` | 6 tests | CSV serialization validation |
| `RangeSerializerParquetTests.cs` | 6 tests | Parquet serialization validation |
| `RangeFinderLoaderTests.cs` | 16 tests | File loading and factory integration |

**Total Serialization Tests: 28**

### Property-Based Tests (1 file)

| File | Tests | Purpose |
|------|-------|---------|
| `CompatibilityTests.cs` | Property-based | Extensive property-based testing |

## Test Architecture

### Independent Functionality Testing

Core tests validate RangeFinder behavior standalone:

- No external library dependencies for core functionality
- Known expected results based on RangeFinder specifications
- Fast execution (< 100ms per test)
- Deterministic, repeatable scenarios

### Compatibility Validation

Compatibility tests ensure consistent behavior:

- Cross-validation against IntervalTree reference implementation
- Large datasets (5K+ items) for comprehensive validation
- Order-independent result validation
- Public API testing only

### Test Data Strategies

#### Controlled Test Data

- Small datasets with known expected results
- Hand-crafted scenarios for boundary testing
- Overlapping ranges to test complex scenarios

#### Generated Test Data

- Time-series patterns simulating real-world temporal data
- Random distributions ensuring broad coverage
- Fixed seeds ensuring reproducible results

## Quality Standards

### Performance Requirements

- Unit tests complete in < 100ms each
- Validation tests complete in < 5 seconds each
- Memory allocation monitored for performance regressions

### Test Naming Conventions

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()

// Examples:
public void Query_PointInMultipleRanges_ReturnsAllContainingRanges()
public void Query_EmptyDataset_ReturnsEmptyResults()
public void Create_FromTuples_CreatesValidInstance()
```

## Testing Patterns

### Parameterized Tests

```csharp
[TestCase(1.5, 2)]  // Point in specific ranges
[TestCase(0.5, 0)]  // Point before all ranges
public void Query_PointQuery_ReturnsCorrectCount(double point, int expected)
```

### Cross-Validation Pattern

```csharp
// Ensure consistent behavior with reference implementation
var rangeFinderResults = rangeFinder.Query(start, end).OrderBy(r => r.Start);
var intervalTreeResults = intervalTree.Query(start, end).OrderBy(r => r.Start);
Assert.That(rangeFinderResults.SequenceEqual(intervalTreeResults));
```

## Dependencies

### Testing Framework

- **NUnit 3.14.0** - Primary testing framework
- **Standard Assert patterns** with fluent syntax

### External Validation

- **IntervalTree** - Used as correctness baseline for compatibility tests
- **Same datasets** used in benchmarks for consistency

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=RangeFinderTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Statistics

- **Total Test Files**: 15 active test files
- **Total Test Methods**: 127+ tests
- **Core Functionality**: 54 tests
- **Data Generation**: 45 tests  
- **Serialization**: 28 tests
- **Property-Based**: Extensive coverage

The test suite provides comprehensive validation of all RangeFinder components, ensuring reliability and correctness across diverse scenarios and usage patterns.
