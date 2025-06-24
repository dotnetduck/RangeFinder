using RangeFinder.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RangeFinder.Tests.Core;

[TestFixture]
public class RangeFinderGenericTypeTests
{
    /// <summary>
    /// Verifies correct behavior for int as the range type parameter.
    /// </summary>
    [Test]
    public void Query_IntegerType_WorksCorrectly()
    {
        var intRanges = new List<NumericRange<int, string>>
        {
            new(1, 5, "Range1"),
            new(3, 7, "Range2"),
            new(10, 15, "Range3")
        };
        var rangeFinder = new RangeFinder<int, string>(intRanges);
        var pointResult = rangeFinder.QueryRanges(4).ToArray();
        Assert.That(pointResult, Has.Length.EqualTo(2));
        var rangeResult = rangeFinder.QueryRanges(2, 6).ToArray();
        Assert.That(rangeResult, Has.Length.EqualTo(2));
    }

    /// <summary>
    /// Verifies correct behavior for double as the range type parameter.
    /// </summary>
    [Test]
    public void Query_DoubleType_WorksCorrectly()
    {
        var doubleRanges = new List<NumericRange<double, string>>
        {
            new(1.0, 2.0, "A"),
            new(1.5, 2.5, "B")
        };
        var rangeFinder = new RangeFinder<double, string>(doubleRanges);
        var result = rangeFinder.QueryRanges(1.7).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.Select(r => r.Value), Contains.Item("A"));
        Assert.That(result.Select(r => r.Value), Contains.Item("B"));
    }

    /// <summary>
    /// Verifies correct behavior for float as the range type parameter.
    /// </summary>
    [Test]
    public void Query_FloatType_WorksCorrectly()
    {
        var floatRanges = new List<NumericRange<float, string>>
        {
            new(1.0f, 2.0f, "A"),
            new(1.5f, 2.5f, "B")
        };
        var rangeFinder = new RangeFinder<float, string>(floatRanges);
        var result = rangeFinder.QueryRanges(1.7f).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.Select(r => r.Value), Contains.Item("A"));
        Assert.That(result.Select(r => r.Value), Contains.Item("B"));
    }

    /// <summary>
    /// Verifies correct behavior for decimal as the range type parameter.
    /// </summary>
    [Test]
    public void Query_DecimalType_WorksCorrectly()
    {
        var decimalRanges = new List<NumericRange<decimal, string>>
        {
            new(1.0m, 2.0m, "A"),
            new(1.5m, 2.5m, "B")
        };
        var rangeFinder = new RangeFinder<decimal, string>(decimalRanges);
        var result = rangeFinder.QueryRanges(1.7m).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.Select(r => r.Value), Contains.Item("A"));
        Assert.That(result.Select(r => r.Value), Contains.Item("B"));
    }
}
