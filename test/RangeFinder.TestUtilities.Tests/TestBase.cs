using RangeFinder.Core;
using RangeFinder.TestUtilities.Generation;
using System.Numerics;
using NUnit.Framework;

namespace RangeFinder.TestUtilities.Tests;

/// <summary>
/// Base class providing common test utilities and test data for IO tests.
/// </summary>
public abstract class TestBase
{
    public enum TestSizes
    {
        Small = 100,
        Medium = 1000,
        Large = 10000
    }

    /// <summary>
    /// All characteristic test cases with parameter objects
    /// </summary>
    public static IEnumerable<object[]> AllCharacteristicTestCases(TestSizes size)
    {
        var count = GetCountForSize(size);
        yield return new object[] { Characteristic.Uniform, RangeParameterFactory.Uniform(count) };
        yield return new object[] { Characteristic.DenseOverlapping, RangeParameterFactory.DenseOverlapping(count) };
        yield return new object[] { Characteristic.SparseNonOverlapping, RangeParameterFactory.SparseNonOverlapping(count) };
        yield return new object[] { Characteristic.Clustered, RangeParameterFactory.Clustered(count) };
    }

    /// <summary>
    /// Converts TestSizes enum to the corresponding int count value
    /// </summary>
    public static int GetCountForSize(TestSizes size) => size switch
    {
        TestSizes.Small => 100,
        TestSizes.Medium => 1000,
        TestSizes.Large => 10000,
        _ => throw new ArgumentException($"Unknown test size: {size}")
    };

    /// <summary>
    /// Numeric type test cases
    /// </summary>
    public static IEnumerable<object[]> NumericTypeTestCases()
    {
        yield return new object[] { typeof(double) };
        yield return new object[] { typeof(float) };
        yield return new object[] { typeof(int) };
        yield return new object[] { typeof(long) };
    }
}

/// <summary>
/// Validation helpers for test data
/// </summary>
public static class Validators
{
    public static void ValidateRangeCollection<TNumber>(
        IEnumerable<NumericRange<TNumber, int>> ranges,
        Parameter parameters,
        string context)
        where TNumber : INumber<TNumber>
    {
        Assert.That(ranges, Is.Not.Null, $"{context}: Ranges should not be null");
        var rangeList = ranges.ToList();
        Assert.That(rangeList.Count, Is.EqualTo(parameters.Count), $"{context}: Count mismatch");
        Assert.That(rangeList, Is.All.Not.Null, $"{context}: All ranges should be non-null");
    }

    public static void ValidateQueryRanges<TNumber>(
        IEnumerable<NumericRange<TNumber, object>> queries,
        Parameter parameters,
        string context)
        where TNumber : INumber<TNumber>
    {
        Assert.That(queries, Is.Not.Null, $"{context}: Queries should not be null");
        var queryList = queries.ToList();
        Assert.That(queryList, Is.All.Not.Null, $"{context}: All queries should be non-null");
    }

    public static void ValidateQueryRanges<TNumber>(
        IEnumerable<NumericRange<TNumber, object>> queries,
        Parameter parameters,
        int expectedCount,
        string context)
        where TNumber : INumber<TNumber>
    {
        Assert.That(queries, Is.Not.Null, $"{context}: Queries should not be null");
        var queryList = queries.ToList();
        Assert.That(queryList, Is.All.Not.Null, $"{context}: All queries should be non-null");
        Assert.That(queryList.Count, Is.EqualTo(expectedCount), $"{context}: Count should match expected");
    }

    public static void ValidateQueryRanges<TNumber>(
        IEnumerable<NumericRange<TNumber, object>> queries,
        Parameter parameters,
        Characteristic characteristic,
        string context)
        where TNumber : INumber<TNumber>
    {
        ValidateQueryRanges(queries, parameters, context);
    }

    public static void ValidateQueryPoints<TNumber>(
        IEnumerable<TNumber> points,
        Parameter parameters,
        string context)
        where TNumber : INumber<TNumber>
    {
        Assert.That(points, Is.Not.Null, $"{context}: Points should not be null");
        var pointList = points.ToList();
        Assert.That(pointList, Is.All.Not.Null, $"{context}: All points should be non-null");
    }

    public static void ValidateQueryPoints<TNumber>(
        IEnumerable<TNumber> points,
        Parameter parameters,
        int expectedCount,
        string context)
        where TNumber : INumber<TNumber>
    {
        Assert.That(points, Is.Not.Null, $"{context}: Points should not be null");
        var pointList = points.ToList();
        Assert.That(pointList, Is.All.Not.Null, $"{context}: All points should be non-null");
        Assert.That(pointList.Count, Is.EqualTo(expectedCount), $"{context}: Count should match expected");
    }

    public static void ValidateQueryPoints<TNumber>(
        IEnumerable<TNumber> points,
        Parameter parameters,
        Characteristic characteristic,
        string context)
        where TNumber : INumber<TNumber>
    {
        ValidateQueryPoints(points, parameters, context);
    }

    public static void ValidateStats(Stats analysis, Parameter parameters, string context)
    {
        Assert.That(analysis, Is.Not.Null, $"{context}: Stats analysis should not be null");
    }

    public static void ValidateCharacteristicSpecificBehavior(Characteristic type, Stats analysis, string context)
    {
        Assert.That(analysis, Is.Not.Null, $"{context}: Analysis should not be null for {type}");
    }
}

/// <summary>
/// Performance testing helpers
/// </summary>
public static class PerformanceHelpers
{
    public static void ValidatePerformanceWithin<T>(Func<T> action, TimeSpan maxDuration, string context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = action();
        stopwatch.Stop();

        Assert.That(stopwatch.Elapsed, Is.LessThan(maxDuration),
            $"{context}: Operation took {stopwatch.Elapsed.TotalMilliseconds}ms, expected < {maxDuration.TotalMilliseconds}ms");
    }

    public static void AssertAcceptablePerformance(TimeSpan elapsed, string context)
    {
        Assert.That(elapsed.TotalSeconds, Is.LessThan(30),
            $"{context}: Operation took {elapsed.TotalSeconds}s, which exceeds acceptable performance threshold");
    }

    public static TimeSpan MeasureExecutionTime(Action action)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    public static void ValidatePerformance(TimeSpan executionTime, TestBase.TestSizes size, string context)
    {
        var maxTime = size switch
        {
            TestBase.TestSizes.Small => TimeSpan.FromMilliseconds(100),
            TestBase.TestSizes.Medium => TimeSpan.FromMilliseconds(500),
            TestBase.TestSizes.Large => TimeSpan.FromSeconds(2),
            _ => TimeSpan.FromSeconds(5)
        };

        Assert.That(executionTime, Is.LessThan(maxTime),
            $"{context}: Execution time {executionTime.TotalMilliseconds}ms exceeded limit for {size} dataset");
    }
}

/// <summary>
/// Factory for creating test parameters with various configurations
/// </summary>
public static class TestParameterFactory
{
    public static Parameter MinimalValidParameters()
    {
        return new Parameter
        {
            Count = 10,
            SpacePerRange = 4.0,
            LengthRatio = 0.4,
            LengthVariability = 0.4,
            OverlapFactor = 1.5,
            ClusteringFactor = 0.2
        };
    }

    public static Parameter MaximalValidParameters()
    {
        return new Parameter
        {
            Count = 100000,
            SpacePerRange = 2.0,
            LengthRatio = 1.5,
            LengthVariability = 0.5,
            OverlapFactor = 3.0,
            ClusteringFactor = 0.3
        };
    }

    public static IEnumerable<(Parameter invalidParams, string expectedError)> InvalidParameterCases()
    {
        yield return (new Parameter { Count = 0, SpacePerRange = 4.0, LengthRatio = 0.4, LengthVariability = 0.4, OverlapFactor = 1.5, ClusteringFactor = 0.2 },
                    "Calculated TotalSpace must be a positive value");
        yield return (new Parameter { Count = -1, SpacePerRange = 4.0, LengthRatio = 0.4, LengthVariability = 0.4, OverlapFactor = 1.5, ClusteringFactor = 0.2 },
                    "Calculated TotalSpace must be a positive value");
        yield return (new Parameter { Count = 100, SpacePerRange = -1.0, LengthRatio = 0.4, LengthVariability = 0.4, OverlapFactor = 1.5, ClusteringFactor = 0.2 },
                    "Calculated TotalSpace must be a positive value");
    }
}
