using IntervalTree;
using RangeFinder.Core;

namespace RangeFinder.RangeTreeCompat.Tests;

/// <summary>
/// Tests to reproduce and fix the specific validator failure bug.
/// </summary>
[TestFixture]
public class IntervalTreeBugTests
{
    [Test]
    public void ValidatorScenario_ExactReproduction()
    {
        // Reproduce the exact scenario that fails in the validator
        var tree = new RangeTreeAdapter<double, int>();
        
        // This reproduces the exact scenario from the failing validator test
        tree.Add(1578.605, 1588.949, 32);  // This should be removed
        tree.Add(1580.0, 1595.0, 790);     // This should remain
        
        // Verify initial state
        var initialResult = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        Assert.That(initialResult, Is.EqualTo(new[] { 32, 790 }), "Initial state should contain both values");
        
        // Remove value 32
        tree.Remove(32);
        
        // Verify count decreased
        Assert.That(tree.Count, Is.EqualTo(1), "Count should decrease after removal");
        
        // Verify values collection no longer contains 32
        var values = tree.Values.ToArray();
        Assert.That(values, Does.Not.Contain(32), "Values should not contain removed value");
        Assert.That(values, Contains.Item(790), "Values should still contain non-removed value");
        
        // This is where the bug occurs - query should only return 790
        var afterRemovalResult = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        Assert.That(afterRemovalResult, Is.EqualTo(new[] { 790 }), 
            "After removal, query should only return remaining overlapping ranges");
    }
    
    [Test]
    public void ComparableWithRangeFinder_ShouldMatchExactly()
    {
        // Create both implementations with identical data
        var tree = new RangeTreeAdapter<double, int>();
        var ranges = new[]
        {
            new NumericRange<double, int>(1578.605, 1588.949, 32),
            new NumericRange<double, int>(1580.0, 1595.0, 790)
        };
        
        // Populate IntervalTree
        foreach (var range in ranges)
        {
            tree.Add(range.Start, range.End, range.Value);
        }
        
        // Create RangeFinder
        var rangeFinder = new RangeFinder<double, int>(ranges);
        
        // Initial query - both should match
        var treeResult1 = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        var finderResult1 = rangeFinder.QueryRanges(1578.605, 1588.949).Select(r => r.Value).OrderBy(x => x).ToArray();
        Assert.That(treeResult1, Is.EqualTo(finderResult1), "Initial query results should match");
        
        // Remove from tree
        tree.Remove(32);
        
        // Create new RangeFinder without the removed value
        var remainingRanges = ranges.Where(r => r.Value != 32);
        var updatedRangeFinder = new RangeFinder<double, int>(remainingRanges);
        
        // After removal - both should match
        var treeResult2 = tree.Query(1578.605, 1588.949).OrderBy(x => x).ToArray();
        var finderResult2 = updatedRangeFinder.QueryRanges(1578.605, 1588.949).Select(r => r.Value).OrderBy(x => x).ToArray();
        Assert.That(treeResult2, Is.EqualTo(finderResult2), "Post-removal query results should match");
    }
    
    [Test]
    public void DebugInternalState_AfterRemoval()
    {
        var tree = new RangeTreeAdapter<double, int>();
        tree.Add(1578.605, 1588.949, 32);
        tree.Add(1580.0, 1595.0, 790);
        
        Console.WriteLine("Before removal:");
        Console.WriteLine($"Count: {tree.Count}");
        Console.WriteLine($"Values: [{string.Join(", ", tree.Values)}]");
        
        tree.Remove(32);
        
        Console.WriteLine("After removal:");
        Console.WriteLine($"Count: {tree.Count}");
        Console.WriteLine($"Values: [{string.Join(", ", tree.Values)}]");
        
        var queryResult = tree.Query(1578.605, 1588.949).ToArray();
        Console.WriteLine($"Query result: [{string.Join(", ", queryResult)}]");
        
        // Also enumerate all RangeValuePairs to see the internal state
        var allPairs = tree.ToList();
        Console.WriteLine($"All pairs: {allPairs.Count}");
        foreach (var pair in allPairs)
        {
            Console.WriteLine($"  Range: [{pair.From}, {pair.To}] = {pair.Value}");
        }
    }
}