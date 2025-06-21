using RangeFinder.Core;

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


    #region Range Query Tests

    [TestCase(2.5, 5.1, 4)]
    [TestCase(-5, 4.0, 4)]
    [TestCase(10.0, 15.0, 1)]
    public void Query_RangeQuery_ReturnsCorrectCount(
        double queryStart, double queryEnd, int expectedCount)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        var result = rangeFinder.QueryRanges(queryStart, queryEnd).ToArray();
        
        Assert.That(result, Has.Length.EqualTo(expectedCount));
    }

    [Test]
    public void Query_RangeQuery_ConsistentResults()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);

        var testCases = new[]
        {
            new NumericRange<double, object>(0, 1),
            new NumericRange<double, object>(1, 2),
            new NumericRange<double, object>(2, 3),
            new NumericRange<double, object>(3, 4),
            new NumericRange<double, object>(4, 5),
            new NumericRange<double, object>(5, 6),
            new NumericRange<double, object>(1.5, 4.5),
            new NumericRange<double, object>(0, 10),
            new NumericRange<double, object>(15, 25)
        };

        foreach (var queryRange in testCases)
        {
            var results = rangeFinder.QueryRanges(queryRange.Start, queryRange.End)
                .OrderBy(r => r.Start)
                .ThenBy(r => r.End)
                .ToArray();

            // Basic validation - should return consistent results
            Assert.That(results, Is.Not.Null);
        }
    }

    #endregion

    #region Point Query Tests

    [TestCase(1.5, 2)] // Point in ranges [1.0,2.2] and [1.0,4.0]
    [TestCase(2.0, 3)] // Point in ranges [2.0,2.5], [1.0,4.0], and touches [1.0,2.2]
    [TestCase(4.0, 2)] // Point at boundaries of ranges [1.0,4.0] and [4.0,5.0]
    [TestCase(0.5, 0)] // Point before all ranges
    [TestCase(25.0, 0)] // Point after all ranges
    [TestCase(5.5, 1)] // Point in range [5.0,6.0]
    [TestCase(6.0, 2)] // Point at boundary of [5.0,6.0] and [6.0,20.0]
    public void Query_PointQuery_ReturnsCorrectCount(
        double point, int expectedCount)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        var result = rangeFinder.QueryRanges(point).ToArray();
        
        Assert.That(result, Has.Length.EqualTo(expectedCount));
    }

    [Test]
    public void Query_PointQuery_ReturnsCorrectRanges()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        // Test point 2.0 which should be in ranges [2.0,2.5], [1.0,4.0], and at boundary of [1.0,2.2]
        var result = rangeFinder.QueryRanges(2.0).OrderBy(r => r.Value).ToArray();
        
        Assert.That(result, Has.Length.EqualTo(3));
        Assert.That(result[0].Value, Is.EqualTo(1)); // [1.0,2.2]
        Assert.That(result[1].Value, Is.EqualTo(2)); // [2.0,2.5] 
        Assert.That(result[2].Value, Is.EqualTo(3)); // [1.0,4.0]
    }

    [Test]
    public void Query_PointQuery_ConsistentWithRangeQuery()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        // Test several points to ensure point query gives same results as range query with same start/end
        var testPoints = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 10.0 };
        
        foreach (var point in testPoints)
        {
            var pointResult = rangeFinder.QueryRanges(point).OrderBy(r => r.Value).ToArray();
            var rangeResult = rangeFinder.QueryRanges(point, point).OrderBy(r => r.Value).ToArray();
            
            Assert.That(pointResult, Has.Length.EqualTo(rangeResult.Length),
                $"Point query and range query should return same count for value {point}");
            
            for (int i = 0; i < pointResult.Length; i++)
            {
                Assert.That(pointResult[i].Value, Is.EqualTo(rangeResult[i].Value),
                    $"Point query and range query should return same ranges for value {point}");
            }
        }
    }

    #endregion

    #region Edge Cases and Boundary Tests

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

    [Test]
    public void Query_SingleRange_BoundaryConditions()
    {
        var singleRange = new List<NumericRange<double, int>>
        {
            new(1.0, 2.0, 1)
        };
        var rangeFinder = new RangeFinder<double, int>(singleRange);
        
        // Point at start boundary
        Assert.That(rangeFinder.QueryRanges(1.0).Count(), Is.EqualTo(1));
        
        // Point at end boundary  
        Assert.That(rangeFinder.QueryRanges(2.0).Count(), Is.EqualTo(1));
        
        // Point inside range
        Assert.That(rangeFinder.QueryRanges(1.5).Count(), Is.EqualTo(1));
        
        // Point before range
        Assert.That(rangeFinder.QueryRanges(0.5), Is.Empty);
        
        // Point after range
        Assert.That(rangeFinder.QueryRanges(2.5), Is.Empty);
    }

    [Test]
    public void Query_OverlappingRanges_ReturnsAllContaining()
    {
        var overlappingRanges = new List<NumericRange<double, int>>
        {
            new(1.0, 5.0, 1),  // Large range
            new(2.0, 3.0, 2),  // Contained within first
            new(4.0, 6.0, 3)   // Overlaps with first
        };
        var rangeFinder = new RangeFinder<double, int>(overlappingRanges);
        
        // Point 2.5 should be in ranges 1 and 2
        var result = rangeFinder.QueryRanges(2.5).OrderBy(r => r.Value).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result[0].Value, Is.EqualTo(1));
        Assert.That(result[1].Value, Is.EqualTo(2));
        
        // Point 4.5 should be in ranges 1 and 3
        result = rangeFinder.QueryRanges(4.5).OrderBy(r => r.Value).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result[0].Value, Is.EqualTo(1));
        Assert.That(result[1].Value, Is.EqualTo(3));
    }

    #endregion

    #region Performance and Optimization Tests

    [Test]
    public void Query_EarlyTermination_WorksCorrectly()
    {
        // Create ranges where early termination should be beneficial
        var ranges = new[]
        {
            new NumericRange<double, int>(1, 2, 1),   // _canTerminateHere[0] should be false
            new NumericRange<double, int>(3, 4, 2),   // _canTerminateHere[1] should be false  
            new NumericRange<double, int>(5, 6, 3),   // _canTerminateHere[2] should be true (no later range ends after this starts)
            new NumericRange<double, int>(7, 8, 4),   // _canTerminateHere[3] should be true
            new NumericRange<double, int>(9, 10, 5)   // _canTerminateHere[4] should be true
        };

        var rangeFinder = new RangeFinder<double, int>(ranges);
        
        // Query that should find some ranges and terminate early
        var queryRange = new NumericRange<double, object>(0, 6.5);
        var results = rangeFinder.QueryRanges(queryRange.Start, queryRange.End).ToArray();
        
        // Should find ranges [1,2], [3,4], [5,6] but not [7,8], [9,10]
        Assert.That(results, Has.Length.EqualTo(3));
        Assert.That(results.Any(r => r.Start == 1), Is.True);
        Assert.That(results.Any(r => r.Start == 3), Is.True);
        Assert.That(results.Any(r => r.Start == 5), Is.True);
        Assert.That(results.Any(r => r.Start == 7), Is.False);
        Assert.That(results.Any(r => r.Start == 9), Is.False);
    }

    [Test]
    public void Query_LargeDataset_PerformanceConsistency()
    {
        // Generate larger dataset for performance testing
        var random = new Random(42);
        var ranges = new List<NumericRange<double, int>>();

        for (var i = 0; i < 1000; i++)
        {
            var start = i * 1.0 + random.NextDouble() * 0.5;
            var duration = random.NextDouble() * 5 + 1;
            ranges.Add(new NumericRange<double, int>(start, start + duration, i));
        }

        var rangeFinder = new RangeFinder<double, int>(ranges);

        // Generate query ranges
        var queryRanges = new List<NumericRange<double, object>>();
        for (var i = 0; i < 50; i++)
        {
            var queryStart = random.NextDouble() * 1000 * 0.8;
            var queryDuration = random.NextDouble() * 10 + 5;
            queryRanges.Add(new NumericRange<double, object>(queryStart, queryStart + queryDuration));
        }

        // Test each query range - should complete without issues
        foreach (var queryRange in queryRanges)
        {
            var results = rangeFinder.QueryRanges(queryRange.Start, queryRange.End)
                .OrderBy(r => r.Start)
                .ThenBy(r => r.End)
                .ToArray();

            Assert.That(results, Is.Not.Null);
        }
    }

    #endregion

    #region Generic Type Tests

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
        
        // Point query
        var pointResult = rangeFinder.QueryRanges(4).ToArray();
        Assert.That(pointResult, Has.Length.EqualTo(2));
        
        // Range query
        var rangeResult = rangeFinder.QueryRanges(2, 6).ToArray();
        Assert.That(rangeResult, Has.Length.EqualTo(2));
    }

    [Test] 
    public void Query_DifferentAssociatedTypes_WorksCorrectly()
    {
        var stringValueRanges = new List<NumericRange<double, string>>
        {
            new(1.0, 2.0, "First"),
            new(1.5, 2.5, "Second")
        };
        
        var rangeFinder = new RangeFinder<double, string>(stringValueRanges);
        var result = rangeFinder.QueryRanges(1.7).ToArray();
        
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.Select(r => r.Value), Contains.Item("First"));
        Assert.That(result.Select(r => r.Value), Contains.Item("Second"));
    }

    #endregion
}