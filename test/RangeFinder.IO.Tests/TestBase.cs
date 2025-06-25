using RangeFinder.Core;
using RangeFinder.IO.Generation;
using System.Numerics;

namespace RangeFinder.IO.Tests;

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
        var count = (int)size;
        yield return new object[] { Characteristic.Uniform, RangeParameterFactory.Uniform(count) };
        yield return new object[] { Characteristic.DenseOverlapping, RangeParameterFactory.DenseOverlapping(count) };
        yield return new object[] { Characteristic.SparseNonOverlapping, RangeParameterFactory.SparseNonOverlapping(count) };
        yield return new object[] { Characteristic.Clustered, RangeParameterFactory.Clustered(count) };
    }

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
}