using RangeFinder.Core;

namespace RangeFinder.Tests.Core;

/// <summary>
/// Tests for LinearRangeFinder to ensure it works correctly as a reference implementation.
/// These tests validate the naive implementation used for back-to-back testing.
/// </summary>
[TestFixture]
internal class LinearRangeFinderTests
{
    [Test]
    public void QueryRanges_SimpleOverlap_ReturnsCorrectRanges()
    {
        // Arrange
        (double, double, string)[] ranges = [
            (1.0, 3.0, "A"),
            (2.0, 4.0, "B"),
            (5.0, 7.0, "C")
        ];
        IEnumerable<NumericRange<double, string>> numericRanges = ranges.Select(r => new NumericRange<double, string>(r.Item1, r.Item2, r.Item3));
        LinearRangeFinder<double, string> finder = new(numericRanges);

        // Act
        List<NumericRange<double, string>> results = [.. finder.QueryRanges(2.5, 3.5)];

        // Assert
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.Select(r => r.Value), Contains.Item("A"));
        Assert.That(results.Select(r => r.Value), Contains.Item("B"));
    }

    [Test]
    public void QueryRanges_PointQuery_ReturnsContainingRanges()
    {
        // Arrange
        (double, double, string)[] ranges = [
            (1.0, 3.0, "A"),
            (2.0, 4.0, "B"),
            (5.0, 7.0, "C")
        ];
        IEnumerable<NumericRange<double, string>> numericRanges = ranges.Select(r => new NumericRange<double, string>(r.Item1, r.Item2, r.Item3));
        LinearRangeFinder<double, string> finder = new(numericRanges);

        // Act
        List<NumericRange<double, string>> results = [.. finder.QueryRanges(2.5)];

        // Assert
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.Select(r => r.Value), Contains.Item("A"));
        Assert.That(results.Select(r => r.Value), Contains.Item("B"));
    }

    [Test]
    public void QueryRanges_NoOverlap_ReturnsEmpty()
    {
        // Arrange
        (double, double, string)[] ranges = [
            (1.0, 2.0, "A"),
            (3.0, 4.0, "B")
        ];
        IEnumerable<NumericRange<double, string>> numericRanges = ranges.Select(r => new NumericRange<double, string>(r.Item1, r.Item2, r.Item3));
        LinearRangeFinder<double, string> finder = new(numericRanges);

        // Act
        List<NumericRange<double, string>> results = [.. finder.QueryRanges(2.5, 2.8)];

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void EmptyDataset_ReturnsEmptyResults()
    {
        // Arrange
        LinearRangeFinder<double, string> finder = new([]);

        // Act
        List<NumericRange<double, string>> results = [.. finder.QueryRanges(1.0, 2.0)];

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(results, Is.Empty);
            Assert.That(finder.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void Properties_CorrectValues()
    {
        // Arrange
        (double, double, string)[] ranges = [
            (1.0, 3.0, "A"),
            (2.0, 4.0, "B"),
            (5.0, 7.0, "C")
        ];
        IEnumerable<NumericRange<double, string>> numericRanges = ranges.Select(r => new NumericRange<double, string>(r.Item1, r.Item2, r.Item3));
        LinearRangeFinder<double, string> finder = new(numericRanges);

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(finder.Count, Is.EqualTo(3));
            Assert.That(finder.LowerBound, Is.EqualTo(1.0));
            Assert.That(finder.UpperBound, Is.EqualTo(7.0));
            Assert.That(finder.Values.Count(), Is.EqualTo(3));
        });
    }

    [Test]
    public void BackToBackTesting_Example()
    {
        // Arrange - Same test data for both implementations
        (double, double, string)[] ranges = [
            (1.0, 3.0, "A"),
            (2.0, 4.0, "B"),
            (3.5, 5.0, "C"),
            (4.5, 6.0, "D")
        ];

        IEnumerable<NumericRange<double, string>> numericRanges = ranges.Select(r => new NumericRange<double, string>(r.Item1, r.Item2, r.Item3));
        LinearRangeFinder<double, string> linearFinder = new(numericRanges);
        RangeFinder<double, string> optimizedFinder = RangeFinderFactory.Create(ranges);

        // Act - Query both implementations
        List<string> linearResults = [.. linearFinder.QueryRanges(3.0, 4.0)
            .Select(r => r.Value)
            .OrderBy(v => v)];

        List<string> optimizedResults = [.. optimizedFinder.QueryRanges(3.0, 4.0)
            .Select(r => r.Value)
            .OrderBy(v => v)];

        // Assert - Results should be identical
        Assert.Multiple(() =>
        {
            Assert.That(optimizedResults, Is.EqualTo(linearResults));
            Assert.That(linearResults, Contains.Item("A"));
            Assert.That(linearResults, Contains.Item("B"));
            Assert.That(linearResults, Contains.Item("C"));
        });
    }
}