using RangeFinder.Core;

namespace RangeFinder.Tests;

/// <summary>
/// Tests for RangeFinderFactory static methods.
/// Validates different factory creation patterns and error handling.
/// </summary>
[TestFixture]
public class RangeFinderFactoryTests
{
    [Test]
    public void Create_FromNumericRanges_CreatesValidInstance()
    {
        var ranges = new[]
        {
            new NumericRange<double, string>(1.0, 2.0, "First"),
            new NumericRange<double, string>(3.0, 4.0, "Second")
        };

        var rangeFinder = RangeFinderFactory.Create(ranges);

        Assert.That(rangeFinder.Count, Is.EqualTo(2));
        Assert.That(rangeFinder.LowerBound, Is.EqualTo(1.0));
        Assert.That(rangeFinder.UpperBound, Is.EqualTo(4.0));
    }

    [Test]
    public void Create_FromNumericRanges_NullInput_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        IEnumerable<NumericRange<double, string>> ranges = null;
        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(ranges));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("ranges"));
    }

    [Test]
    public void Create_FromTuples_CreatesValidInstance()
    {
        var ranges = new[]
        {
            (1.0, 2.0, "First"),
            (3.0, 4.0, "Second"),
            (2.5, 3.5, "Third")
        };

        var rangeFinder = RangeFinderFactory.Create(ranges);

        Assert.That(rangeFinder.Count, Is.EqualTo(3));
        
        var results = rangeFinder.QueryRanges(2.7).ToArray();
        Assert.That(results, Has.Length.EqualTo(1));
        Assert.That(results[0].Value, Is.EqualTo("Third"));
    }

    [Test]
    public void Create_FromTuples_NullInput_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        IEnumerable<(double, double, string)> ranges = null;
        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(ranges));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("ranges"));
    }

    [Test]
    public void Create_FromTuplesWithoutValues_CreatesInstanceWithIndices()
    {
        var ranges = new[]
        {
            (1.0, 2.0),
            (3.0, 4.0),
            (2.5, 3.5)
        };

        var rangeFinder = RangeFinderFactory.Create(ranges);

        Assert.That(rangeFinder.Count, Is.EqualTo(3));
        
        var results = rangeFinder.QueryRanges(2.7).ToArray();
        Assert.That(results, Has.Length.EqualTo(1));
        Assert.That(results[0].Value, Is.EqualTo(2)); // Third range has index 2
    }

    [Test]
    public void Create_FromTuplesWithoutValues_NullInput_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        IEnumerable<(double, double)> ranges = null;
        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(ranges));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("ranges"));
    }

    [Test]
    public void Create_FromArrays_CreatesValidInstance()
    {
        var starts = new[] { 1.0, 3.0, 2.5 };
        var ends = new[] { 2.0, 4.0, 3.5 };
        var values = new[] { "First", "Second", "Third" };

        var rangeFinder = RangeFinderFactory.Create(starts, ends, values);

        Assert.That(rangeFinder.Count, Is.EqualTo(3));
        
        var results = rangeFinder.QueryRanges(2.7).ToArray();
        Assert.That(results, Has.Length.EqualTo(1));
        Assert.That(results[0].Value, Is.EqualTo("Third"));
    }

    [Test]
    public void Create_FromArrays_NullStarts_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        double[] starts = null;
        var ends = new[] { 2.0, 4.0 };
        var values = new[] { "First", "Second" };

        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(starts, ends, values));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("starts"));
    }

    [Test]
    public void Create_FromArrays_NullEnds_ThrowsArgumentNullException()
    {
        var starts = new[] { 1.0, 3.0 };
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        double[] ends = null;
        var values = new[] { "First", "Second" };

        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(starts, ends, values));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("ends"));
    }

    [Test]
    public void Create_FromArrays_NullValues_ThrowsArgumentNullException()
    {
        var starts = new[] { 1.0, 3.0 };
        var ends = new[] { 2.0, 4.0 };
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        string[] values = null;

        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(starts, ends, values));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("values"));
    }

    [Test]
    public void Create_FromArrays_DifferentLengths_ThrowsArgumentException()
    {
        var starts = new[] { 1.0, 3.0 };
        var ends = new[] { 2.0, 4.0, 5.0 }; // Different length
        var values = new[] { "First", "Second" };

        var ex = Assert.Throws<ArgumentException>(() => RangeFinderFactory.Create(starts, ends, values));
        Assert.That(ex.Message, Contains.Substring("same length"));
    }

    [Test]
    public void Create_FromStartEndArrays_CreatesInstanceWithIndices()
    {
        var starts = new[] { 1.0, 3.0, 2.5 };
        var ends = new[] { 2.0, 4.0, 3.5 };

        var rangeFinder = RangeFinderFactory.Create(starts, ends);

        Assert.That(rangeFinder.Count, Is.EqualTo(3));
        
        var results = rangeFinder.QueryRanges(2.7).ToArray();
        Assert.That(results, Has.Length.EqualTo(1));
        Assert.That(results[0].Value, Is.EqualTo(2)); // Third range has index 2
    }

    [Test]
    public void Create_FromStartEndArrays_NullStarts_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        double[] starts = null;
        var ends = new[] { 2.0, 4.0 };

        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(starts, ends));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("starts"));
    }

    [Test]
    public void Create_FromStartEndArrays_NullEnds_ThrowsArgumentNullException()
    {
        var starts = new[] { 1.0, 3.0 };
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        double[] ends = null;

        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderFactory.Create(starts, ends));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("ends"));
    }

    [Test]
    public void Create_FromStartEndArrays_DifferentLengths_ThrowsArgumentException()
    {
        var starts = new[] { 1.0, 3.0 };
        var ends = new[] { 2.0, 4.0, 5.0 }; // Different length

        var ex = Assert.Throws<ArgumentException>(() => RangeFinderFactory.Create(starts, ends));
        Assert.That(ex.Message, Contains.Substring("same length"));
    }

    [Test]
    public void CreateEmpty_CreatesEmptyInstance()
    {
        var rangeFinder = RangeFinderFactory.CreateEmpty<double, string>();

        Assert.That(rangeFinder.Count, Is.EqualTo(0));
        Assert.That(rangeFinder.QueryRanges(1.0, 2.0), Is.Empty);
        Assert.That(rangeFinder.QueryRanges(1.5), Is.Empty);
    }

    [Test]
    public void CreateEmpty_WithDifferentTypes_WorksCorrectly()
    {
        var intRangeFinder = RangeFinderFactory.CreateEmpty<int, string>();
        var floatRangeFinder = RangeFinderFactory.CreateEmpty<float, object>();

        Assert.That(intRangeFinder.Count, Is.EqualTo(0));
        Assert.That(floatRangeFinder.Count, Is.EqualTo(0));
    }

    [Test]
    public void Create_IntegerTypes_WorksCorrectly()
    {
        var ranges = new[]
        {
            (1, 5, "First"),
            (3, 7, "Second"),
            (10, 15, "Third")
        };

        var rangeFinder = RangeFinderFactory.Create(ranges);

        Assert.That(rangeFinder.Count, Is.EqualTo(3));
        
        var results = rangeFinder.QueryRanges(4).ToArray();
        Assert.That(results, Has.Length.EqualTo(2));
        Assert.That(results.Select(r => r.Value), Contains.Item("First"));
        Assert.That(results.Select(r => r.Value), Contains.Item("Second"));
    }

    [Test]
    public void Create_EmptyCollections_WorksCorrectly()
    {
        var emptyRanges = Enumerable.Empty<NumericRange<double, string>>();
        var emptyTuples = Enumerable.Empty<(double, double, string)>();
        var emptySimpleTuples = Enumerable.Empty<(double, double)>();

        var rangeFinder1 = RangeFinderFactory.Create(emptyRanges);
        var rangeFinder2 = RangeFinderFactory.Create(emptyTuples);
        var rangeFinder3 = RangeFinderFactory.Create(emptySimpleTuples);

        Assert.That(rangeFinder1.Count, Is.EqualTo(0));
        Assert.That(rangeFinder2.Count, Is.EqualTo(0));
        Assert.That(rangeFinder3.Count, Is.EqualTo(0));
    }

    [Test]
    public void Create_EmptyArrays_WorksCorrectly()
    {
        var emptyStarts = new double[0];
        var emptyEnds = new double[0];
        var emptyValues = new string[0];

        var rangeFinder1 = RangeFinderFactory.Create(emptyStarts, emptyEnds, emptyValues);
        var rangeFinder2 = RangeFinderFactory.Create(emptyStarts, emptyEnds);

        Assert.That(rangeFinder1.Count, Is.EqualTo(0));
        Assert.That(rangeFinder2.Count, Is.EqualTo(0));
    }
}