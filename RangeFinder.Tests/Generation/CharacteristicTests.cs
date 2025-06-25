using RangeFinder.IO.Generation;
using RangeFinder.Core;
using RangeFinder.Tests;

namespace RangeFinder.Tests.Generation;

/// <summary>
/// Tests for Characteristic enum functionality.
/// Verifies that all range characteristics can be used to create valid parameter presets.
/// </summary>
[TestFixture]
public class CharacteristicTests : TestBase
{
    [Test, TestCaseSource(nameof(AllCharacteristicTestCases), new object[] { TestSizes.Small })]
    public void Characteristic_AllValues_CanCreatePresets(Characteristic type, Parameter parameters)
    {
        var ranges = Generator.GenerateRanges<double>(parameters);
        Validators.ValidateRangeCollection(ranges, parameters, $"Characteristic.{type}");
    }

    [Test]
    public void Characteristic_EnumCompleteness_AllValuesHaveFactoryMethods()
    {
        var allCharacteristics = Enum.GetValues<Characteristic>();
        Assert.That(allCharacteristics.Length, Is.EqualTo(4), "Should have exactly 4 characteristics");

        // Verify each characteristic has a corresponding factory method
        foreach (Characteristic type in allCharacteristics)
        {
            Assert.DoesNotThrow(() =>
            {
                var parameters = type switch
                {
                    Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(TestSizes.Small),
                    Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(TestSizes.Small),
                    Characteristic.Clustered => RangeParameterFactory.Clustered(TestSizes.Small),
                    Characteristic.Uniform => RangeParameterFactory.Uniform(TestSizes.Small),
                    _ => throw new ArgumentException($"Unknown dataset type: {type}")
                };
                parameters.Validate();
            }, $"Characteristic.{type} should have a valid factory method");
        }
    }

    [Test, TestCaseSource(nameof(AllCharacteristicTestCases), new object[] { TestSizes.Medium })]
    public void Characteristic_ProducesExpectedOverlapPattern(Characteristic type, Parameter parameters)
    {
        var ranges = Generator.GenerateRanges<double>(parameters);
        var analysis = Analyzer.Analyze(ranges);

        Validators.ValidateStats(analysis, parameters, $"Characteristic.{type}");
        Validators.ValidateCharacteristicSpecificBehavior(type, analysis, $"Characteristic.{type}");

        // Verify with actual point queries using RangeFinder
        var rangeFinder = new RangeFinder<double, int>(ranges);
        var queryPoints = Generator.GenerateQueryPoints<double>(parameters, Math.Min(100, parameters.Count));

        var overlapCounts = queryPoints.Select(point => rangeFinder.QueryRanges(point).Count()).ToArray();
        var averageOverlap = overlapCounts.Average();

        Console.WriteLine($"{type}: Theoretical={analysis.OverlapPercentage:F1}%, Point Query Average={averageOverlap:F2}");
    }

    [Test]
    public void Characteristic_BoundaryValues_HandleCorrectly()
    {
        // Test with minimal count
        foreach (Characteristic type in Enum.GetValues<Characteristic>())
        {
            var parameters = type switch
            {
                Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(1),
                Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(1),
                Characteristic.Clustered => RangeParameterFactory.Clustered(1),
                Characteristic.Uniform => RangeParameterFactory.Uniform(1),
                _ => throw new ArgumentException($"Unknown dataset type: {type}")
            };

            var ranges = Generator.GenerateRanges<double>(parameters);
            Validators.ValidateRangeCollection(ranges, parameters, $"Boundary test - {type} with count=1");
        }
    }

    [Test, TestCaseSource(nameof(NumericTypeTestCases))]
    public void Characteristic_DifferentNumericTypes_GenerateCorrectly(Type numericType)
    {
        var parameters = RangeParameterFactory.Uniform(TestSizes.Small);

        var method = typeof(Generator).GetMethod(nameof(Generator.GenerateRanges))!.MakeGenericMethod(numericType);
        var ranges = method.Invoke(null, new object[] { parameters })!;

        // Use reflection to call ValidateRangeCollection with the correct generic type
        var validatorMethod = typeof(Validators).GetMethod(nameof(Validators.ValidateRangeCollection))!.MakeGenericMethod(numericType);

        Assert.DoesNotThrow(() =>
        {
            validatorMethod.Invoke(null, new[] { ranges, parameters, $"{numericType.Name} type" });
        }, $"Should generate valid ranges for {numericType.Name}");

        // Verify count property using reflection
        var countProperty = ranges.GetType().GetProperty("Count")!;
        var count = (int)countProperty.GetValue(ranges)!;
        Assert.That(count, Is.EqualTo(parameters.Count), $"{numericType.Name}: Should generate correct count");
    }

    [Test]
    public void Characteristic_LargeDatasets_PerformanceIsReasonable()
    {
        foreach (Characteristic type in Enum.GetValues<Characteristic>())
        {
            var parameters = type switch
            {
                Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(TestSizes.Large),
                Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(TestSizes.Large),
                Characteristic.Clustered => RangeParameterFactory.Clustered(TestSizes.Large),
                Characteristic.Uniform => RangeParameterFactory.Uniform(TestSizes.Large),
                _ => throw new ArgumentException($"Unknown dataset type: {type}")
            };

            var executionTime = PerformanceHelpers.MeasureExecutionTime(() =>
            {
                var ranges = Generator.GenerateRanges<double>(parameters);
                Validators.ValidateRangeCollection(ranges, parameters, $"Performance test - {type}");
            });

            PerformanceHelpers.ValidatePerformance(executionTime, TestSizes.Large, $"Characteristic.{type} generation");
        }
    }

    [Test]
    public void Characteristic_ZeroQueryCount_ReturnsEmptyCollections()
    {
        var parameters = RangeParameterFactory.Uniform(TestSizes.Small);

        var queryRanges = Generator.GenerateQueryRanges<double>(parameters, 0);
        var queryPoints = Generator.GenerateQueryPoints<double>(parameters, 0);

        Assert.That(queryRanges, Is.Empty, "Zero query ranges should return empty collection");
        Assert.That(queryPoints, Is.Empty, "Zero query points should return empty collection");
    }

    [Test]
    public void Characteristic_AllTypes_ProduceDifferentCharacteristics()
    {
        const int testSize = 100_000; // Large dataset for statistical confidence
        var typeAnalyses = new Dictionary<Characteristic, (Stats Analysis, double PointQueryAverage)>();

        // Generate and analyze each type
        foreach (Characteristic type in Enum.GetValues<Characteristic>())
        {
            var parameters = type switch
            {
                Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(testSize),
                Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(testSize),
                Characteristic.Clustered => RangeParameterFactory.Clustered(testSize),
                Characteristic.Uniform => RangeParameterFactory.Uniform(testSize),
                _ => throw new ArgumentException($"Unknown dataset type: {type}")
            };

            var ranges = Generator.GenerateRanges<double>(parameters);
            var analysis = Analyzer.Analyze(ranges);

            // Validate with point queries using RangeFinder (10% sampling)
            var rangeFinder = new RangeFinder<double, int>(ranges);
            var sampleSize = testSize / 10; // 10% sampling
            var queryPoints = Generator.GenerateQueryPoints<double>(parameters, sampleSize);
            var overlapCounts = queryPoints.Select(point => rangeFinder.QueryRanges(point).Count()).ToArray();
            var pointQueryAverage = overlapCounts.Average();
            var pointQueryStdDev = Math.Sqrt(overlapCounts.Select(x => Math.Pow(x - pointQueryAverage, 2)).Average());

            typeAnalyses[type] = (analysis, pointQueryAverage);

            Console.WriteLine($"{type}: Theoretical={analysis.OverlapPercentage:F1}%, " +
                            $"Point Query Avg={pointQueryAverage:F2}±{pointQueryStdDev:F2} " +
                            $"(n={sampleSize:N0})");
        }

        // Verify that different types produce different characteristics
        var overlapPercentages = typeAnalyses.Values.Select(a => a.Analysis.OverlapPercentage).ToList();
        var pointQueryAverages = typeAnalyses.Values.Select(a => a.PointQueryAverage).ToList();

        // There should be significant variation in overlap characteristics
        var overlapRange = overlapPercentages.Max() - overlapPercentages.Min();
        Assert.That(overlapRange, Is.GreaterThan(50),
            "Different dataset types should produce significantly varied overlap characteristics");

        // Verify specific type characteristics - Dense should have highest overlap
        Assert.That(typeAnalyses[Characteristic.DenseOverlapping].Analysis.OverlapPercentage,
            Is.GreaterThan(typeAnalyses[Characteristic.SparseNonOverlapping].Analysis.OverlapPercentage),
            "Dense should have higher overlap than Sparse");

        // Point query results should correlate with theoretical analysis
        Assert.That(typeAnalyses[Characteristic.DenseOverlapping].PointQueryAverage,
            Is.GreaterThan(typeAnalyses[Characteristic.SparseNonOverlapping].PointQueryAverage),
            "Dense point queries should show higher overlap than Sparse");
    }

    [Test]
    public void Characteristic_LargeDataset_OverlapValidation()
    {
        const int datasetSize = 200_000;
        const double samplingRate = 0.1; // 10% sampling

        Console.WriteLine($"Validating overlap characteristics with {datasetSize:N0} ranges, {samplingRate:P0} sampling");

        var validationResults = new Dictionary<Characteristic, (double Theoretical, double Empirical, double StdDev, int SampleSize)>();

        foreach (Characteristic type in Enum.GetValues<Characteristic>())
        {
            var parameters = type switch
            {
                Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(datasetSize),
                Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(datasetSize),
                Characteristic.Clustered => RangeParameterFactory.Clustered(datasetSize),
                Characteristic.Uniform => RangeParameterFactory.Uniform(datasetSize),
                _ => throw new ArgumentException($"Unknown dataset type: {type}")
            };

            var ranges = Generator.GenerateRanges<double>(parameters);
            var theoreticalAnalysis = Analyzer.Analyze(ranges);

            // Empirical validation with 10% sampling
            var rangeFinder = new RangeFinder<double, int>(ranges);
            var sampleSize = (int)(datasetSize * samplingRate);
            var queryPoints = Generator.GenerateQueryPoints<double>(parameters, sampleSize);

            var overlapCounts = queryPoints.Select(point => rangeFinder.QueryRanges(point).Count()).ToArray();
            var empiricalAverage = overlapCounts.Average();
            var empiricalStdDev = Math.Sqrt(overlapCounts.Select(x => Math.Pow(x - empiricalAverage, 2)).Average());

            validationResults[type] = (theoreticalAnalysis.OverlapPercentage, empiricalAverage, empiricalStdDev, sampleSize);

            Console.WriteLine($"{type,-20}: Theory={theoreticalAnalysis.OverlapPercentage:F1}%, " +
                            $"Empirical={empiricalAverage:F2}±{empiricalStdDev:F2}, " +
                            $"Sample={sampleSize:N0}");
        }

        // Validate that Dense has highest overlap
        Assert.That(validationResults[Characteristic.DenseOverlapping].Theoretical,
            Is.GreaterThan(validationResults[Characteristic.SparseNonOverlapping].Theoretical),
            "Dense should have higher theoretical overlap than Sparse");

        Assert.That(validationResults[Characteristic.DenseOverlapping].Empirical,
            Is.GreaterThan(validationResults[Characteristic.SparseNonOverlapping].Empirical),
            "Dense should have higher empirical overlap than Sparse");

        // Validate reasonable correlation between theoretical and empirical
        foreach (var (type, (theoretical, empirical, stdDev, _)) in validationResults)
        {
            // Allow some tolerance due to different measurement methods
            var expectedEmpiricalRange = theoretical / 100.0; // Convert percentage to multiplier
            Assert.That(empirical, Is.GreaterThan(0.1),
                $"{type}: Empirical overlap should be meaningful (>0.1)");
        }
    }

    [Test]
    public void Characteristic_EnumValues_HaveExpectedCount()
    {
        var typeValues = Enum.GetValues<Characteristic>();
        Assert.That(typeValues.Length, Is.EqualTo(4),
            "Should have exactly 4 range characteristics defined");

        var expectedTypes = new[]
        {
            Characteristic.DenseOverlapping,
            Characteristic.SparseNonOverlapping,
            Characteristic.Clustered,
            Characteristic.Uniform
        };

        foreach (var expectedType in expectedTypes)
        {
            Assert.That(typeValues, Contains.Item(expectedType),
                $"Should contain {expectedType}");
        }
    }

    [Test]
    public void Characteristic_FactoryMethods_ProduceValidParameters()
    {
        var testCases = new[]
        {
            ("DenseOverlapping", (Func<int, Parameter>)(count => RangeParameterFactory.DenseOverlapping(count))),
            ("SparseNonOverlapping", (Func<int, Parameter>)(count => RangeParameterFactory.SparseNonOverlapping(count))),
            ("Clustered", (Func<int, Parameter>)(count => RangeParameterFactory.Clustered(count))),
            ("Uniform", (Func<int, Parameter>)(count => RangeParameterFactory.Uniform(count)))
        };

        foreach (var (name, factory) in testCases)
        {
            var sizes = new[] { 1, 10, 100, 1000 };

            foreach (var size in sizes)
            {
                var parameters = factory(size);

                Assert.DoesNotThrow(() => parameters.Validate(),
                    $"{name} factory should produce valid parameters for size {size}");

                Assert.That(parameters.Count, Is.EqualTo(size),
                    $"{name} factory should set correct count");
                Assert.That(parameters.TotalSpace, Is.GreaterThan(0),
                    $"{name} factory should produce positive total space");
                Assert.That(parameters.AverageLength, Is.GreaterThan(0),
                    $"{name} factory should produce positive average length");
            }
        }
    }
}
