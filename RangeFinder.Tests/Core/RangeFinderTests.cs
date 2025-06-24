using RangeFinder.Core;
using RangeFinder.Tests.Helper;

namespace RangeFinder.Tests.Core;

/// <summary>
/// Pillar 1: Independent functionality tests for RangeFinder.
/// Tests RangeFinder behavior standalone without external library dependencies.
/// Validates correctness according to RangeFinder's own specifications.
/// </summary>
[TestFixture]
public class RangeFinderTests
{
    private static readonly List<NumericRange<double, int>> TestRanges = new()
    {
        new(1.0, 2.2, 1), new(2.0, 2.5, 2),
        new(1.0, 4.0, 3), new(4.0, 5.0, 4),
        new(5.0, 6.0, 5), new(6.0, 20.0, 6)
    };

    private static readonly List<NumericRange<double, int>> TestRangesWithNegatives = new()
    {
        new(-10.0, -5.0, 101),
        new(-3.0, 2.0, 102),
        new(-1.0, 1.0, 103),
        new(0.0, 5.0, 104),
        new(3.0, 8.0, 105),
        new(-8.0, -2.0, 106),
        new(-15.0, 15.0, 107)
    };

    public static IEnumerable<TestCaseData> RangeQueryCases()
    {
        // positive only
        yield return new TestCaseData(2.5, 5.1, "overlap in middle (TestRanges)", TestRanges);
        yield return new TestCaseData(-5, 4.0, "query before and into range (TestRanges)", TestRanges);
        yield return new TestCaseData(10.0, 15.0, "inside large range only (TestRanges)", TestRanges);
        // negative only
        yield return new TestCaseData(-12.0, -1.0, "covers multiple negative ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-5.0, 0.0, "boundary at zero (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-2.0, 2.0, "crossing zero (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(0.0, 10.0, "zero to positive (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-20.0, 20.0, "covers all ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-6.0, -6.0, "point query in negative range (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-100.0, -50.0, "outside all ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-9.0, -6.0, "purely negative, excludes positive (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(2.0, -2.0, "empty interval (start > end) (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(double.MinValue, double.MaxValue, "extreme values, should cover all (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(0.0, 0.0, "single point at zero (TestRangesWithNegatives)", TestRangesWithNegatives);
    }

    public static IEnumerable<TestCaseData> PointQueryCases()
    {
        // positive only
        yield return new TestCaseData(1.5, "point in two ranges (TestRanges)", TestRanges);
        yield return new TestCaseData(2.0, "point in three ranges (TestRanges)", TestRanges);
        yield return new TestCaseData(4.0, "point at boundary (TestRanges)", TestRanges);
        yield return new TestCaseData(0.5, "point before all ranges (TestRanges)", TestRanges);
        yield return new TestCaseData(25.0, "point after all ranges (TestRanges)", TestRanges);
        yield return new TestCaseData(5.5, "point in one range (TestRanges)", TestRanges);
        yield return new TestCaseData(6.0, "point at boundary of two ranges (TestRanges)", TestRanges);
        // negative only
        yield return new TestCaseData(-7.0, "inside two negative ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-3.0, "at boundary of two negative ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(0.0, "at zero, inside three ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-1.0, "inside multiple crossing zero (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(-12.0, "outside all negative ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(100.0, "far outside all ranges (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(double.MinValue, "extreme negative (TestRangesWithNegatives)", TestRangesWithNegatives);
        yield return new TestCaseData(double.MaxValue, "extreme positive (TestRangesWithNegatives)", TestRangesWithNegatives);
    }

    /// <summary>
    /// Compares RangeFinder and LinearRangeFinder for a variety of range queries, including positive, negative, and edge cases.
    /// </summary>
    [Test, TestCaseSource(nameof(RangeQueryCases))]
    public void Query_RangeQuery(double queryStart, double queryEnd, string intention, IEnumerable<NumericRange<double, int>> ranges)
    {
        RangeFinder<double, int> rangeFinder = new(ranges);
        LinearRangeFinder<double, int> linearRangeFinder = new(ranges);

        int[] expectedValues = [.. linearRangeFinder.QueryRanges(queryStart, queryEnd).Select(r => r.Value)];
        int[] actualValues = [.. rangeFinder.QueryRanges(queryStart, queryEnd).Select(r => r.Value)];
        SetDifference<int> difference = actualValues.CompareAsSets(expectedValues);
        Assert.That(difference.AreEqual, Is.True, $"[{intention}] Query [{queryStart}, {queryEnd}] failed. {difference.GetDescription()}");
    }

    /// <summary>
    /// Compares RangeFinder and LinearRangeFinder for a variety of point queries, including positive, negative, and edge cases.
    /// </summary>
    [Test, TestCaseSource(nameof(PointQueryCases))]
    public void Query_PointQuery(double point, string intention, IEnumerable<NumericRange<double, int>> ranges)
    {
        RangeFinder<double, int> rangeFinder = new(ranges);
        LinearRangeFinder<double, int> linearRangeFinder = new(ranges);

        int[] actual = [.. rangeFinder.QueryRanges(point).Select(r => r.Value)];
        int[] expected = [.. linearRangeFinder.QueryRanges(point).Select(r => r.Value)];
        SetDifference<int> difference = actual.CompareAsSets(expected);
        Assert.That(difference.AreEqual, Is.True, $"[{intention}] Point query at {point} failed. {difference.GetDescription()}");
    }

}
