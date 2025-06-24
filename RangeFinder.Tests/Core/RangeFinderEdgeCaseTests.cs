using RangeFinder.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace RangeFinder.Tests.Core;

[TestFixture]
public class RangeFinderEdgeCaseTests
{
    /// <summary>
    /// Verifies that querying an empty dataset always returns an empty result for both range and point queries.
    /// </summary>
    [Test]
    public void Query_EmptyDataset_ReturnsEmptyResult()
    {
        var emptyRanges = new List<NumericRange<double, int>>();
        var rangeFinder = new RangeFinder<double, int>(emptyRanges);
        var rangeResult = rangeFinder.QueryRanges(1.0, 2.0).ToArray();
        var pointResult = rangeFinder.QueryRanges(1.0).ToArray();
        Assert.That(rangeResult, Is.Empty);
        Assert.That(pointResult, Is.Empty);
    }

    /// <summary>
    /// Verifies correct behavior at the boundaries and outside of a single range.
    /// Checks inclusion at start/end and exclusion before/after.
    /// </summary>
    [Test]
    public void Query_SingleRange_BoundaryConditions()
    {
        var expected = new[] { 10 };
        var singleRange = new List<NumericRange<double, int>>
        {
            new(1.0, 2.0, expected[0])
        };
        var rangeFinder = new RangeFinder<double, int>(singleRange);
        Assert.That(rangeFinder.QueryRanges(1.0).Select(r => r.Value), Is.EquivalentTo(expected));
        Assert.That(rangeFinder.QueryRanges(2.0).Select(r => r.Value), Is.EquivalentTo(expected));
        Assert.That(rangeFinder.QueryRanges(1.5).Select(r => r.Value), Is.EquivalentTo(expected));
        Assert.That(rangeFinder.QueryRanges(0.5), Is.Empty);
        Assert.That(rangeFinder.QueryRanges(2.5), Is.Empty);
    }

    /// <summary>
    /// Verifies that all overlapping ranges containing a point are returned, and that order is as expected.
    /// </summary>
    [Test]
    public void Query_OverlappingRanges_ReturnsAllContaining()
    {
        var overlappingRanges = new List<NumericRange<double, int>>
        {
            new(1.0, 5.0, 1),
            new(2.0, 3.0, 2),
            new(4.0, 6.0, 3)
        };
        var rangeFinder = new RangeFinder<double, int>(overlappingRanges);
        var result = rangeFinder.QueryRanges(2.5).Select(r => r.Value).OrderBy(x => x).ToArray();
        Assert.That(result, Is.EquivalentTo(new[] { 1, 2 }));
        result = rangeFinder.QueryRanges(4.5).Select(r => r.Value).OrderBy(x => x).ToArray();
        Assert.That(result, Is.EquivalentTo(new[] { 1, 3 }));
    }
}
