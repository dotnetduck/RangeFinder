using RangeFinder.Core;
using RangeFinder.IO.Generation;
using RangeFinder.Tests;
using System.Reflection;

namespace RangeFinder.Tests.Generation;

/// <summary>
/// Tests for query generation functionality.
/// Verifies that query ranges and points are generated correctly for testing datasets.
/// </summary>
[TestFixture]
public class QueryGenerationTests : TestBase
{
    [Test, TestCaseSource(nameof(AllCharacteristicTestCases), new object[] { TestSizes.Medium })]
    public void QueryGenerator_ProducesValidQueries(Characteristic type, Parameter parameters)
    {
        const int queryCount = 50;
        var ranges = Generator.GenerateRanges<double>(parameters);
        var queryRanges = Generator.GenerateQueryRanges<double>(parameters, queryCount);
        var queryPoints = Generator.GenerateQueryPoints<double>(parameters, queryCount);

        Validators.ValidateRangeCollection(ranges, parameters, $"{type} data generation");
        Validators.ValidateQueryRanges(queryRanges, parameters, queryCount, $"{type} query ranges");
        Validators.ValidateQueryPoints(queryPoints, parameters, queryCount, $"{type} query points");
    }

    [Test, TestCaseSource(nameof(NumericTypeTestCases))]
    public void QueryGenerator_DifferentNumericTypes_GenerateCorrectly(Type numericType)
    {
        // Use smaller parameters for small numeric types to avoid overflow
        var isSmallType = numericType == typeof(sbyte) || numericType == typeof(byte) ||
                         numericType == typeof(short) || numericType == typeof(ushort);

        var parameters = isSmallType
            ? RangeParameterFactory.Custom(count: 10, spacePerRange: 1.0, lengthRatio: 0.5, overlapFactor: 1.0, lengthVariability: 0.1, clusteringFactor: 0.1)
            : RangeParameterFactory.Custom(count: TestSizes.Small, spacePerRange: 10.0, lengthRatio: 1.0, overlapFactor: 2.0, lengthVariability: 0.25, clusteringFactor: 0.3);

        const int queryCount = 25;

        var queryRangeMethod = typeof(Generator).GetMethod(nameof(Generator.GenerateQueryRanges))!.MakeGenericMethod(numericType);
        var queryPointMethod = typeof(Generator).GetMethod(nameof(Generator.GenerateQueryPoints))!.MakeGenericMethod(numericType);

        var queryRanges = queryRangeMethod.Invoke(null, new object[] { parameters, queryCount, 2.0 })!;
        var queryPoints = queryPointMethod.Invoke(null, new object[] { parameters, queryCount })!;

        // Validate collections using reflection
        var rangeValidatorMethod = typeof(Validators).GetMethod(nameof(Validators.ValidateQueryRanges))!.MakeGenericMethod(numericType);
        var pointValidatorMethod = typeof(Validators).GetMethod(nameof(Validators.ValidateQueryPoints))!.MakeGenericMethod(numericType);

        Assert.DoesNotThrow(() =>
        {
            rangeValidatorMethod.Invoke(null, new[] { queryRanges, parameters, queryCount, $"{numericType.Name} query ranges" });
            pointValidatorMethod.Invoke(null, new[] { queryPoints, parameters, queryCount, $"{numericType.Name} query points" });
        }, $"Should generate valid {numericType.Name} queries");
    }

    [Test]
    public void QueryGenerator_VariableQueryLengthMultiplier_ProducesCorrectSizes()
    {
        var parameters = RangeParameterFactory.Uniform(TestSizes.Small);
        var multipliers = new[] { 0.5, 1.0, 2.0, 5.0 };

        foreach (var multiplier in multipliers)
        {
            var queryRanges = Generator.GenerateQueryRanges<double>(parameters, 20, multiplier);

            Assert.That(queryRanges, Has.Count.EqualTo(20),
                $"Multiplier {multiplier}: Should generate correct count");

            // Verify that ranges scale with multiplier (larger multiplier = potentially larger ranges)
            var averageQueryLength = queryRanges.Average(q => q.End - q.Start);
            var expectedBaseLength = parameters.AverageLength;

            if (multiplier >= 1.0)
            {
                Assert.That(averageQueryLength, Is.GreaterThanOrEqualTo(expectedBaseLength * 0.5),
                    $"Multiplier {multiplier}: Query ranges should scale with multiplier");
            }

            Console.WriteLine($"Multiplier {multiplier}: Avg query length = {averageQueryLength:F2}, Expected base = {expectedBaseLength:F2}");
        }
    }

    [Test]
    public void QueryGenerator_BoundaryQueryCounts_HandleCorrectly()
    {
        var parameters = RangeParameterFactory.Uniform(TestSizes.Small);

        // Test zero queries
        var zeroQueryRanges = Generator.GenerateQueryRanges<double>(parameters, 0);
        var zeroQueryPoints = Generator.GenerateQueryPoints<double>(parameters, 0);

        Assert.That(zeroQueryRanges, Is.Empty, "Zero queries should return empty ranges");
        Assert.That(zeroQueryPoints, Is.Empty, "Zero queries should return empty points");

        // Test single query
        var singleQueryRanges = Generator.GenerateQueryRanges<double>(parameters, 1);
        var singleQueryPoints = Generator.GenerateQueryPoints<double>(parameters, 1);

        Validators.ValidateQueryRanges(singleQueryRanges, parameters, 1, "Single query ranges");
        Validators.ValidateQueryPoints(singleQueryPoints, parameters, 1, "Single query points");

        // Test large query count
        var largeQueryCount = TestSizes.Medium;
        var largeQueryRanges = Generator.GenerateQueryRanges<double>(parameters, largeQueryCount);
        var largeQueryPoints = Generator.GenerateQueryPoints<double>(parameters, largeQueryCount);

        Validators.ValidateQueryRanges(largeQueryRanges, parameters, largeQueryCount, "Large query ranges");
        Validators.ValidateQueryPoints(largeQueryPoints, parameters, largeQueryCount, "Large query points");
    }

    [Test]
    public void QueryGenerator_RepeatableResults_WithSameSeed()
    {
        var parameters = RangeParameterFactory.Uniform(TestSizes.Small);
        const int queryCount = 10;

        var queryRanges1 = Generator.GenerateQueryRanges<double>(parameters, queryCount);
        var queryRanges2 = Generator.GenerateQueryRanges<double>(parameters, queryCount);

        var queryPoints1 = Generator.GenerateQueryPoints<double>(parameters, queryCount);
        var queryPoints2 = Generator.GenerateQueryPoints<double>(parameters, queryCount);

        Assert.That(queryRanges1.Count, Is.EqualTo(queryRanges2.Count), "Same seed should produce same range count");
        Assert.That(queryPoints1.Count, Is.EqualTo(queryPoints2.Count), "Same seed should produce same point count");

        for (int i = 0; i < queryRanges1.Count; i++)
        {
            Assert.That(queryRanges1[i].Start, Is.EqualTo(queryRanges2[i].Start).Within(1e-10),
                $"Query range {i} start should be identical");
            Assert.That(queryRanges1[i].End, Is.EqualTo(queryRanges2[i].End).Within(1e-10),
                $"Query range {i} end should be identical");
        }

        for (int i = 0; i < queryPoints1.Count; i++)
        {
            Assert.That(queryPoints1[i], Is.EqualTo(queryPoints2[i]).Within(1e-10),
                $"Query point {i} should be identical");
        }
    }

    [Test]
    public void QueryGenerator_PerformanceRegression_CompletesWithinReasonableTime()
    {
        var parameters = RangeParameterFactory.Uniform(TestSizes.Large);
        const int largeQueryCount = TestSizes.Large;

        var executionTime = PerformanceHelpers.MeasureExecutionTime(() =>
        {
            var queryRanges = Generator.GenerateQueryRanges<double>(parameters, largeQueryCount);
            var queryPoints = Generator.GenerateQueryPoints<double>(parameters, largeQueryCount);

            Validators.ValidateQueryRanges(queryRanges, parameters, largeQueryCount, "Performance test ranges");
            Validators.ValidateQueryPoints(queryPoints, parameters, largeQueryCount, "Performance test points");
        });

        PerformanceHelpers.ValidatePerformance(executionTime, largeQueryCount, "Large query generation");
    }

    [Test]
    public void QueryGenerator_DifferentSeeds_ProduceDifferentResults()
    {
        var baseParams = RangeParameterFactory.Uniform(TestSizes.Small);
        var params1 = baseParams with { RandomSeed = 111 };
        var params2 = baseParams with { RandomSeed = 222 };

        const int queryCount = 10;

        var queryRanges1 = Generator.GenerateQueryRanges<double>(params1, queryCount);
        var queryRanges2 = Generator.GenerateQueryRanges<double>(params2, queryCount);

        var queryPoints1 = Generator.GenerateQueryPoints<double>(params1, queryCount);
        var queryPoints2 = Generator.GenerateQueryPoints<double>(params2, queryCount);

        Assert.That(queryRanges1.Count, Is.EqualTo(queryRanges2.Count), "Different seeds should produce same count");
        Assert.That(queryPoints1.Count, Is.EqualTo(queryPoints2.Count), "Different seeds should produce same count");

        // Should find differences in ranges
        bool foundRangeDifference = false;
        for (int i = 0; i < Math.Min(5, queryRanges1.Count) && !foundRangeDifference; i++)
        {
            if (Math.Abs(queryRanges1[i].Start - queryRanges2[i].Start) > 1e-10 ||
                Math.Abs(queryRanges1[i].End - queryRanges2[i].End) > 1e-10)
            {
                foundRangeDifference = true;
            }
        }

        // Should find differences in points
        bool foundPointDifference = false;
        for (int i = 0; i < Math.Min(5, queryPoints1.Count) && !foundPointDifference; i++)
        {
            if (Math.Abs(queryPoints1[i] - queryPoints2[i]) > 1e-10)
            {
                foundPointDifference = true;
            }
        }

        Assert.That(foundRangeDifference, Is.True, "Different seeds should produce different query ranges");
        Assert.That(foundPointDifference, Is.True, "Different seeds should produce different query points");
    }

    [Test]
    public void QueryGenerator_EdgeCases_HandleGracefully()
    {
        // Test with very small total space
        var tinyParams = RangeParameterFactory.Custom(
            count: 2,
            spacePerRange: 0.01,
            lengthRatio: 0.5,
            overlapFactor: 1.0,
            lengthVariability: 0.0,
            clusteringFactor: 0.0);

        var tinyQueryRanges = Generator.GenerateQueryRanges<double>(tinyParams, 5);
        var tinyQueryPoints = Generator.GenerateQueryPoints<double>(tinyParams, 5);

        Validators.ValidateQueryRanges(tinyQueryRanges, tinyParams, 5, "Tiny space query ranges");
        Validators.ValidateQueryPoints(tinyQueryPoints, tinyParams, 5, "Tiny space query points");

        // Test with extreme query length multiplier
        var extremeQueryRanges = Generator.GenerateQueryRanges<double>(tinyParams, 3, 10.0);

        Assert.That(extremeQueryRanges, Has.Count.EqualTo(3), "Extreme multiplier should still produce correct count");

        foreach (var query in extremeQueryRanges)
        {
            Assert.That(query.Start, Is.GreaterThanOrEqualTo(0), "Query start should be non-negative");
            Assert.That(query.End, Is.LessThanOrEqualTo(tinyParams.TotalSpace), "Query end should be within bounds");
            Assert.That(query.Start, Is.LessThanOrEqualTo(query.End), "Query should be valid");
        }
    }
}
