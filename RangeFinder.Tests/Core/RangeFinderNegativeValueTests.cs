using IntervalTree;
using RangeFinder.Core;
using RangeFinder.Tests.Helper;

namespace RangeFinder.Tests.Core;

/// <summary>
/// Tests for RangeFinder with negative values to ensure proper handling of negative ranges.
/// These tests complement property-based testing by providing deterministic coverage
/// of critical boundary conditions that random generation might miss consistently.
/// Property-based testing relies on random value generation and may not reliably
/// test negative value edge cases across all test runs.
/// </summary>
[TestFixture]
public class RangeFinderNegativeValueTests
{
    /// <summary>
    /// Test data with negative values to ensure proper handling of negative ranges.
    /// Added to complement property-based testing which relies on random generation
    /// and may not consistently cover critical boundary conditions like negative values.
    /// </summary>
    private static readonly List<NumericRange<double, int>> TestRangesWithNegatives = new()
    {
        new(-10.0, -5.0, 101),   // Completely negative range
        new(-3.0, 2.0, 102),     // Negative start, positive end (crosses zero)
        new(-1.0, 1.0, 103),     // Symmetric around zero
        new(0.0, 5.0, 104),      // Starting at zero
        new(3.0, 8.0, 105),      // Positive range
        new(-8.0, -2.0, 106),    // Another negative range
        new(-15.0, 15.0, 107)    // Wide range crossing zero
    };

    [TestCase(-12.0, -1.0)]  // Query covering multiple negative ranges
    [TestCase(-5.0, 0.0)]    // Query from negative to zero
    [TestCase(-2.0, 2.0)]    // Query crossing zero
    [TestCase(0.0, 10.0)]    // Query from zero to positive
    [TestCase(-20.0, 20.0)]  // Query covering all ranges
    [TestCase(-6.0, -6.0)]   // Point query in negative range
    [TestCase(-100.0, -50.0)]// Query outside all ranges (negative)
    public void Query_NegativeValues_ReturnsCorrectRanges(
        double queryStart, double queryEnd)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        var intervalTree = new IntervalTree<double, int>();
        foreach (var range in TestRangesWithNegatives)
            intervalTree.Add(range.Start, range.End, range.Value);

        var expectedValues = intervalTree.Query(queryStart, queryEnd).ToArray();
        var actualRanges = rangeFinder.QueryRanges(queryStart, queryEnd).ToArray();
        var actualValues = actualRanges.Select(r => r.Value).ToArray();
        var difference = actualValues.CompareAsSets(expectedValues);

        if (!difference.AreEqual)
        {
            var rangeData = TestRangesWithNegatives.Select(r => (r.Start, r.End)).ToArray();
            difference.PrintRangeDebugInfo($"Range query [{queryStart}, {queryEnd}]", (queryStart, queryEnd), rangeData);
        }

        Assert.That(difference.AreEqual, Is.True, 
            $"Query [{queryStart}, {queryEnd}] failed. Expected: [{string.Join(", ", expectedValues)}]. {difference.GetDescription()}");
    }

    [TestCase(-7.0)]  // Point in ranges [-10.0,-5.0] and [-8.0,-2.0]
    [TestCase(-3.0)]  // Point at boundary: ranges [-3.0,2.0] and [-8.0,-2.0]
    [TestCase(0.0)]   // Point at zero: ranges [-3.0,2.0], [-1.0,1.0], [0.0,5.0]
    [TestCase(-1.0)]  // Point in multiple ranges crossing zero
    [TestCase(-12.0)] // Point outside all ranges (negative)
    public void Query_NegativePointValues_MatchesIntervalTree(double point)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        var intervalTree = new IntervalTree<double, int>();
        foreach (var range in TestRangesWithNegatives)
            intervalTree.Add(range.Start, range.End, range.Value);

        var actual = rangeFinder.QueryRanges(point).ToArray();
        var expected = intervalTree.Query(point, point);
        Assert.That(actual.Length, Is.EqualTo(expected.Count()),
            $"Point query at {point} should return {expected.Count()} ranges, but got {actual.Length}");
    }

    [Test]
    public void Query_NegativeValues_CorrectSpecificRanges()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        var intervalTree = new IntervalTree<double, int>();
        foreach (var range in TestRangesWithNegatives)
            intervalTree.Add(range.Start, range.End, range.Value);

        // Test point -7.0 which should be in ranges [-10.0,-5.0] and [-8.0,-2.0]
        var actualValues = rangeFinder.QueryRanges(-7.0).Select(r => r.Value).ToArray();
        var expectedValues = intervalTree.Query(-7.0, -7.0);

        var difference = actualValues.CompareAsSets(expectedValues);
        Assert.IsTrue(difference.AreEqual, $"Point query at -7.0 failed. {difference.GetDescription()}");
    }

    [Test]
    public void Query_CrossingZero_HandlesNegativeToPositiveCorrectly()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        var intervalTree = new IntervalTree<double,int>();
        foreach (var range in TestRangesWithNegatives)
            intervalTree.Add(range.Start, range.End, range.Value);
        var actualValues = rangeFinder.Query(-0.5, 0.5).ToArray();
        var expectedValues = intervalTree.Query(-0.5, 0.5);
        var difference = actualValues.CompareAsSets(expectedValues);
        Assert.IsTrue(difference.AreEqual, $"Crossing zero query failed. {difference.GetDescription()}");
    }

    [Test]
    public void Query_PurelyNegativeRange_ExcludesPositiveRanges()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        var intervalTree = new IntervalTree<double, int>();
        foreach (var range in TestRangesWithNegatives)
            intervalTree.Add(range.Start, range.End, range.Value);
        var actualValues = rangeFinder.QueryRanges(-9.0, -6.0).Select(r => r.Value).ToArray();
        var expectedValues = intervalTree.Query(-9.0, -6.0);
        var difference = actualValues.CompareAsSets(expectedValues);
        Assert.IsTrue(difference.AreEqual, $"Purely negative query [-9.0, -6.0] failed. {difference.GetDescription()}");
    }
}