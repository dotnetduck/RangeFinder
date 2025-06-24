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
        NumericRange<double, string>[] ranges = [
            new(1.0, 3.0, "A"),
            new(2.0, 4.0, "B"),
            new(5.0, 7.0, "C")
        ];
        var finder = new LinearRangeFinder<double, string>(ranges);

        // Act
        var results = finder.QueryRanges(2.5, 3.5).ToList();

        // Assert
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.Select(r => r.Value), Contains.Item("A"));
        Assert.That(results.Select(r => r.Value), Contains.Item("B"));
    }

    [Test]
    public void QueryRanges_PointQuery_ReturnsContainingRanges()
    {
        // Arrange
        NumericRange<double, string>[] ranges = [
            new(1.0, 3.0, "A"),
            new(2.0, 4.0, "B"),
            new(5.0, 7.0, "C")
        ];
        var finder = new LinearRangeFinder<double, string>(ranges);

        // Act
        var results = finder.QueryRanges(2.5).ToList();

        // Assert
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.Select(r => r.Value), Contains.Item("A"));
        Assert.That(results.Select(r => r.Value), Contains.Item("B"));
    }

    [Test]
    public void QueryRanges_NoOverlap_ReturnsEmpty()
    {
        // Arrange
        NumericRange<double, string>[] ranges = [
            new(1.0, 2.0, "A"),
            new(3.0, 4.0, "B")
        ];
        var finder = new LinearRangeFinder<double, string>(ranges);

        // Act
        var results = finder.QueryRanges(2.5, 2.8).ToList();

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void EmptyDataset_ReturnsEmptyResults()
    {
        // Arrange
        var finder = new LinearRangeFinder<double, string>([]);

        // Act
        var results = finder.QueryRanges(1.0, 2.0).ToList();

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
        NumericRange<double, string>[] ranges = [
            new(1.0, 3.0, "A"),
            new(2.0, 4.0, "B"),
            new(5.0, 7.0, "C")
        ];
        var finder = new LinearRangeFinder<double, string>(ranges);

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
        NumericRange<double, string>[] ranges = [
            new(1.0, 3.0, "A"),
            new(2.0, 4.0, "B"),
            new(3.5, 5.0, "C"),
            new(4.5, 6.0, "D")
        ];

        var linearFinder = new LinearRangeFinder<double, string>(ranges);
        var optimizedFinder = new RangeFinder<double, string>(ranges);

        // Act - Query both implementations
        var linearResults = linearFinder.QueryRanges(3.0, 4.0)
            .Select(r => r.Value)
            .OrderBy(v => v).ToList();

        var optimizedResults = optimizedFinder.QueryRanges(3.0, 4.0)
            .Select(r => r.Value)
            .OrderBy(v => v).ToList();

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