using RangeFinder.RangeTreeCompat;
using RangeFinder.Core;

namespace RangeFinder.RangeTreeCompat.Tests;

/// <summary>
/// Edge case and regression tests for the RangeTree compatibility wrapper.
/// These tests target specific issues found during validation.
/// </summary>
[TestFixture]
public class IntervalTreeEdgeCaseTests
{
    [Test]
    public void DuplicateValues_Remove_ShouldRemoveAllInstances()
    {
        var tree = new RangeTreeAdapter<int, int>();
        tree.Add(1, 5, 32);   // Same value 32
        tree.Add(10, 15, 32); // Same value 32
        tree.Add(20, 25, 790);
        
        Assert.That(tree.Count, Is.EqualTo(3));
        
        tree.Remove(32);
        
        Assert.That(tree.Count, Is.EqualTo(1), "Should remove all instances of duplicate value");
        
        var values = tree.Values.ToList();
        Assert.That(values, Does.Not.Contain(32), "Should not contain any instance of 32");
        Assert.That(values, Contains.Item(790));
        Assert.That(values.Count(v => v == 32), Is.EqualTo(0), "Should have no instances of 32");
    }

    [Test]
    public void DuplicateValues_RemoveAll_ShouldRemoveAllInstances()
    {
        var tree = new RangeTreeAdapter<int, int>();
        tree.Add(1, 5, 32);   // Same value 32
        tree.Add(10, 15, 32); // Same value 32
        tree.Add(8, 12, 32);  // Same value 32
        tree.Add(20, 25, 790);
        
        Assert.That(tree.Count, Is.EqualTo(4));
        
        tree.Remove(new[] { 32 });
        
        Assert.That(tree.Count, Is.EqualTo(1), "Should remove all instances of 32");
        Assert.That(tree.Values, Is.EquivalentTo(new[] { 790 }));
    }

    [Test]
    public void ConsecutiveRemoveOperations_ShouldMaintainCorrectState()
    {
        var tree = new RangeTreeAdapter<double, int>();
        
        // Add ranges similar to the failing validator test
        tree.Add(1578.0, 1590.0, 32);   // Overlaps with query [1578.605, 1588.949]
        tree.Add(1580.0, 1595.0, 790);  // Overlaps with query [1578.605, 1588.949]
        tree.Add(1585.0, 1600.0, 100);  // Overlaps with query [1578.605, 1588.949]
        tree.Add(1570.0, 1580.0, 200);  // Overlaps with query [1578.605, 1588.949] (1570≤1588.949 && 1578.605≤1580)
        
        // First removal
        tree.Remove(32);
        
        var query1Result = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        Assert.That(query1Result, Is.EqualTo(new[] { 100, 200, 790 }), 
            "After removing 32, query should return all remaining overlapping ranges");
        
        // Second removal  
        tree.Remove(100);
        
        var query2Result = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        Assert.That(query2Result, Is.EqualTo(new[] { 200, 790 }), 
            "After removing 100, query should return remaining overlapping ranges");
    }

    [Test]
    public void LazyReconstruction_ShouldWorkCorrectlyAfterRemoval()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 10, "A");
        tree.Add(5, 15, "B");
        tree.Add(12, 20, "C");
        
        // Query to trigger initial construction
        var initialQuery = tree.Query(8).ToList();
        Assert.That(initialQuery, Is.EquivalentTo(new[] { "A", "B" }));
        
        // Remove an item to mark as dirty
        tree.Remove("A");
        
        // Query should trigger reconstruction and return correct results
        var afterRemovalQuery = tree.Query(8).ToList();
        Assert.That(afterRemovalQuery, Is.EquivalentTo(new[] { "B" }));
        
        // Another query should use the already-reconstructed finder
        var secondQuery = tree.Query(14).ToList();
        Assert.That(secondQuery, Is.EquivalentTo(new[] { "B", "C" }));
    }

    [Test]
    public void RemoveFromEmptyTree_ShouldNotThrow()
    {
        var tree = new IntervalTree<int, string>();
        
        Assert.DoesNotThrow(() => tree.Remove("NonExistent"));
        Assert.DoesNotThrow(() => tree.Remove(new[] { "A", "B" }));
        
        Assert.That(tree.Count, Is.EqualTo(0));
    }

    [Test]
    public void QueryAfterClearAndRepopulate_ShouldReturnCorrectResults()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        
        var initialQuery = tree.Query(4).ToList();
        Assert.That(initialQuery, Is.EquivalentTo(new[] { "A", "B" }));
        
        tree.Clear();
        
        var afterClearQuery = tree.Query(4).ToList();
        Assert.That(afterClearQuery, Is.Empty);
        
        tree.Add(2, 6, "X");
        tree.Add(5, 9, "Y");
        
        var afterRepopulateQuery = tree.Query(4).ToList();
        Assert.That(afterRepopulateQuery, Is.EquivalentTo(new[] { "X" }));
        
        var fullRangeQuery = tree.Query(6).ToList();
        Assert.That(fullRangeQuery, Is.EquivalentTo(new[] { "X", "Y" }));
    }

    [Test]
    public void EdgeRanges_ShouldHandleBoundaryConditions()
    {
        var tree = new IntervalTree<double, string>();
        tree.Add(0.0, 10.0, "A");
        tree.Add(10.0, 20.0, "B");  // Touching ranges
        tree.Add(20.0, 30.0, "C");
        
        // Query at exact boundaries
        var queryAt10 = tree.Query(10.0).ToList();
        Assert.That(queryAt10, Is.EquivalentTo(new[] { "A", "B" }), 
            "Should include both ranges that touch at point 10");
        
        var queryAt20 = tree.Query(20.0).ToList();
        Assert.That(queryAt20, Is.EquivalentTo(new[] { "B", "C" }), 
            "Should include both ranges that touch at point 20");
        
        // Range queries at boundaries
        var rangeQuery = tree.Query(9.0, 11.0).ToList();
        Assert.That(rangeQuery, Is.EquivalentTo(new[] { "A", "B" }));
    }

    [Test]
    public void LargeDataset_RemovalPattern_ShouldMaintainConsistency()
    {
        var tree = new RangeTreeAdapter<double, int>();
        var rangeFinder = new RangeFinder<double, int>(Enumerable.Empty<NumericRange<double, int>>());
        
        // Create a scenario similar to the validator failure
        var ranges = new List<(double, double, int)>
        {
            (1578.0, 1590.0, 32),
            (1580.0, 1595.0, 790),
            (1585.0, 1600.0, 100),
            (1570.0, 1580.0, 200),
            (1590.0, 1605.0, 300),
            (1575.0, 1585.0, 400)
        };
        
        // Add to both
        foreach (var (start, end, value) in ranges)
        {
            tree.Add(start, end, value);
        }
        
        var numericRanges = ranges.Select(r => new NumericRange<double, int>(r.Item1, r.Item2, r.Item3));
        rangeFinder = new RangeFinder<double, int>(numericRanges);
        
        // Test initial state
        var initialTreeResult = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        var initialFinderResult = rangeFinder.QueryRanges(1578.605, 1588.949)
            .Select(r => r.Value).OrderBy(x => x).ToArray();
        
        Assert.That(initialTreeResult, Is.EqualTo(initialFinderResult), 
            "Initial state should match between tree and finder");
        
        // Remove some values
        var toRemove = new[] { 32, 100 };
        tree.Remove(toRemove);
        
        var remainingRanges = numericRanges.Where(r => !toRemove.Contains(r.Value));
        rangeFinder = new RangeFinder<double, int>(remainingRanges);
        
        // Test after removal
        var afterRemovalTreeResult = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        var afterRemovalFinderResult = rangeFinder.QueryRanges(1578.605, 1588.949)
            .Select(r => r.Value).OrderBy(x => x).ToArray();
        
        Assert.That(afterRemovalTreeResult, Is.EqualTo(afterRemovalFinderResult), 
            "Results should match after removal operations");
    }
}