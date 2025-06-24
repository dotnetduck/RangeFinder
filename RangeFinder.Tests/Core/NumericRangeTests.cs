using RangeFinder.Core;

namespace RangeFinder.Tests.Core;

/// <summary>
/// Tests for NumericRange struct to validate overlap and containment logic.
/// These tests ensure the core range operations work correctly.
/// </summary>
[TestFixture]
public class NumericRangeTests
{
    public static IEnumerable<TestCaseData> OverlapsWithRangeCases()
    {
        yield return new TestCaseData("overlapping ranges", 1.0, 5.0, 3.0, 7.0, true);
        yield return new TestCaseData("touching boundaries", 1.0, 3.0, 3.0, 5.0, true);
        yield return new TestCaseData("non-overlapping ranges", 1.0, 2.0, 3.0, 4.0, false);
        yield return new TestCaseData("identical ranges", 1.0, 3.0, 1.0, 3.0, true);
        yield return new TestCaseData("first contains second", 1.0, 10.0, 3.0, 7.0, true);
        yield return new TestCaseData("second contains first", 3.0, 7.0, 1.0, 10.0, true);
        yield return new TestCaseData("zero-width ranges touching", 3.0, 3.0, 3.0, 3.0, true);
        yield return new TestCaseData("zero-width ranges separate", 2.0, 2.0, 4.0, 4.0, false);
        yield return new TestCaseData("negative ranges overlapping", -10.0, -5.0, -7.0, -2.0, true);
        yield return new TestCaseData("negative ranges non-overlapping", -10.0, -8.0, -5.0, -3.0, false);
    }

    public static IEnumerable<TestCaseData> OverlapsWithQueryRangeCases()
    {
        yield return new TestCaseData("query overlaps start", 2.0, 6.0, 1.0, 3.0, true);
        yield return new TestCaseData("query overlaps end", 2.0, 6.0, 5.0, 8.0, true);
        yield return new TestCaseData("query inside range", 2.0, 6.0, 3.0, 4.0, true);
        yield return new TestCaseData("query contains range", 2.0, 6.0, 1.0, 8.0, true);
        yield return new TestCaseData("query touches start", 2.0, 6.0, 1.0, 2.0, true);
        yield return new TestCaseData("query touches end", 2.0, 6.0, 6.0, 8.0, true);
        yield return new TestCaseData("query before range", 3.0, 5.0, 1.0, 2.0, false);
        yield return new TestCaseData("query after range", 3.0, 5.0, 6.0, 8.0, false);
        yield return new TestCaseData("zero-width query at point", 3.0, 3.0, 3.0, 3.0, true);
        yield return new TestCaseData("zero-width query contains point", 3.0, 3.0, 2.0, 4.0, true);
    }

    public static IEnumerable<TestCaseData> ContainsCases()
    {
        yield return new TestCaseData("point inside range", 1.0, 5.0, 3.0, true);
        yield return new TestCaseData("point at start boundary", 2.0, 8.0, 2.0, true);
        yield return new TestCaseData("point at end boundary", 2.0, 8.0, 8.0, true);
        yield return new TestCaseData("point before range", 3.0, 7.0, 1.0, false);
        yield return new TestCaseData("point after range", 3.0, 7.0, 9.0, false);
        yield return new TestCaseData("point just before start", 3.0, 7.0, 2.9, false);
        yield return new TestCaseData("point just after end", 3.0, 7.0, 7.1, false);
        yield return new TestCaseData("zero-width range contains exact point", 5.0, 5.0, 5.0, true);
        yield return new TestCaseData("zero-width range does not contain other points", 5.0, 5.0, 4.9, false);
        yield return new TestCaseData("negative range contains point", -10.0, -5.0, -7.0, true);
        yield return new TestCaseData("negative range start boundary", -10.0, -5.0, -10.0, true);
        yield return new TestCaseData("negative range end boundary", -10.0, -5.0, -5.0, true);
        yield return new TestCaseData("negative range point before", -10.0, -5.0, -15.0, false);
        yield return new TestCaseData("negative range point after", -10.0, -5.0, 0.0, false);
    }

    [Test, TestCaseSource(nameof(OverlapsWithRangeCases))]
    public void Overlaps_WithOtherRange(string intention, double start1, double end1, double start2, double end2, bool expected)
    {
        var range1 = new NumericRange<double, string>(start1, end1, "A");
        var range2 = new NumericRange<double, string>(start2, end2, "B");

        Assert.That(range1.Overlaps(range2), Is.EqualTo(expected), $"[{intention}] Range1[{start1}, {end1}] overlaps Range2[{start2}, {end2}]");
        Assert.That(range2.Overlaps(range1), Is.EqualTo(expected), $"[{intention}] Range2[{start2}, {end2}] overlaps Range1[{start1}, {end1}] (symmetry)");
    }

    [Test, TestCaseSource(nameof(OverlapsWithQueryRangeCases))]
    public void Overlaps_WithQueryRange(string intention, double rangeStart, double rangeEnd, double queryStart, double queryEnd, bool expected)
    {
        var range = new NumericRange<double, string>(rangeStart, rangeEnd, "A");

        Assert.That(range.Overlaps(queryStart, queryEnd), Is.EqualTo(expected), 
            $"[{intention}] Range[{rangeStart}, {rangeEnd}] overlaps query[{queryStart}, {queryEnd}]");
    }

    [Test, TestCaseSource(nameof(ContainsCases))]
    public void Contains_Point(string intention, double rangeStart, double rangeEnd, double point, bool expected)
    {
        var range = new NumericRange<double, string>(rangeStart, rangeEnd, "A");

        Assert.That(range.Contains(point), Is.EqualTo(expected), 
            $"[{intention}] Range[{rangeStart}, {rangeEnd}] contains point {point}");
    }

    [Test]
    public void Overlaps_And_Contains_ConsistentBehavior()
    {
        // If a point is contained in a range, then a zero-width range at that point should overlap
        var range = new NumericRange<double, string>(2.0, 8.0, "A");
        double testPoint = 5.0;

        bool containsPoint = range.Contains(testPoint);
        bool overlapsPoint = range.Overlaps(testPoint, testPoint);

        Assert.That(containsPoint, Is.EqualTo(overlapsPoint), 
            "Contains and Overlaps should be consistent for point queries");
    }

    [Test]
    public void IntegerRanges_WorkCorrectly()
    {
        var range = new NumericRange<int, string>(10, 20, "A");

        Assert.Multiple(() =>
        {
            Assert.That(range.Contains(15), Is.True, "Contains point inside range");
            Assert.That(range.Contains(10), Is.True, "Contains start boundary");
            Assert.That(range.Contains(20), Is.True, "Contains end boundary");
            Assert.That(range.Contains(5), Is.False, "Does not contain point before range");
            Assert.That(range.Contains(25), Is.False, "Does not contain point after range");
            Assert.That(range.Overlaps(15, 25), Is.True, "Overlaps with query range");
        });
    }
}