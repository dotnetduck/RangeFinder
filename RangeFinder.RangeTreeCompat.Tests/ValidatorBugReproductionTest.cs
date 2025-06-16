using IntervalTree;
using RangeFinder.Core;

namespace RangeFinder.RangeTreeCompat.Tests;

/// <summary>
/// Test to reproduce the exact validator failure found in continuous testing.
/// Validator reported: IntervalTree returning [92, 2968] when RangeFinder returns [2968]
/// after removal operations - value 92 should have been removed.
/// </summary>
[TestFixture]
public class ValidatorBugReproductionTest
{
    [Test]
    public void ReproduceValidatorFailure_SparseNonOverlapping_RemovalBug()
    {
        // This will attempt to reproduce the exact scenario where
        // RangeFinder returns [2968] but IntervalTree returns [92, 2968]
        // The value 92 should have been removed but still appears
        
        var tree = new RangeTreeAdapter<double, int>();
        
        // We need to add ranges that would overlap with query [4644.875, 4652.362]
        // and then remove some values to create the discrepancy
        
        // Add ranges that overlap with the failing query range
        tree.Add(4640.0, 4650.0, 92);   // This should be removed but persists
        tree.Add(4645.0, 4655.0, 2968); // This should remain
        
        // Verify initial state
        var initialResult = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        Assert.That(initialResult, Is.EqualTo(new[] { 92, 2968 }), 
            "Initial state should contain both values");
        
        // Remove value 92 (this is where the bug occurs)
        tree.Remove(92);
        
        // Verify internal state
        Assert.That(tree.Count, Is.EqualTo(1), "Count should be 1 after removal");
        Assert.That(tree.Values.ToArray(), Is.EqualTo(new[] { 2968 }), 
            "Values should only contain 2968");
        
        // This is the failing assertion - query should only return [2968]
        var afterRemovalResult = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        Assert.That(afterRemovalResult, Is.EqualTo(new[] { 2968 }), 
            "After removal, query should only return [2968], not [92, 2968]");
    }
    
    [Test]
    public void CompareWithRangeFinder_ShouldMatchExactly()
    {
        // Create both implementations and verify they return identical results
        var tree = new RangeTreeAdapter<double, int>();
        var ranges = new[]
        {
            new NumericRange<double, int>(4640.0, 4650.0, 92),
            new NumericRange<double, int>(4645.0, 4655.0, 2968)
        };
        
        // Populate IntervalTree
        foreach (var range in ranges)
        {
            tree.Add(range.Start, range.End, range.Value);
        }
        
        // Create RangeFinder
        var rangeFinder = new RangeFinder<double, int>(ranges);
        
        // Initial query - both should match
        var treeResult1 = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        var finderResult1 = rangeFinder.QueryRanges(4644.875, 4652.362)
            .Select(r => r.Value).OrderBy(x => x).ToArray();
        Assert.That(treeResult1, Is.EqualTo(finderResult1), "Initial results should match");
        
        // Remove from tree
        tree.Remove(92);
        
        // Create new RangeFinder without the removed value
        var remainingRanges = ranges.Where(r => r.Value != 92);
        var updatedRangeFinder = new RangeFinder<double, int>(remainingRanges);
        
        // After removal - this is where the bug manifests
        var treeResult2 = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        var finderResult2 = updatedRangeFinder.QueryRanges(4644.875, 4652.362)
            .Select(r => r.Value).OrderBy(x => x).ToArray();
        
        Console.WriteLine($"Tree result: [{string.Join(", ", treeResult2)}]");
        Console.WriteLine($"Finder result: [{string.Join(", ", finderResult2)}]");
        
        Assert.That(treeResult2, Is.EqualTo(finderResult2), 
            "Post-removal results should match between tree and finder");
    }
    
    [Test]
    public void ReproduceValidatorSequence_DynamicOperations()
    {
        // Attempt to reproduce the exact validator sequence that leads to the bug
        var tree = new RangeTreeAdapter<double, int>();
        
        // Add initial ranges (simulating the validator's initial population)
        tree.Add(4640.0, 4650.0, 92);
        tree.Add(4645.0, 4655.0, 2968);
        tree.Add(4635.0, 4645.0, 100); // Additional range for complexity
        
        // Initial query verification
        var initialResult = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        Console.WriteLine($"Initial result: [{string.Join(", ", initialResult)}]");
        
        // Phase 1: Individual removal (this is where AfterRemove fails in validator)
        tree.Remove(100); // Remove a value first
        
        var afterFirstRemoval = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        Console.WriteLine($"After removing 100: [{string.Join(", ", afterFirstRemoval)}]");
        
        // Phase 2: This is the critical removal that causes the validator failure
        tree.Remove(92);
        
        // Debug internal state
        Console.WriteLine($"Count after removing 92: {tree.Count}");
        Console.WriteLine($"Values after removing 92: [{string.Join(", ", tree.Values)}]");
        
        var afterSecondRemoval = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        Console.WriteLine($"After removing 92: [{string.Join(", ", afterSecondRemoval)}]");
        
        // This should only contain [2968] but validator reports [92, 2968]
        Assert.That(afterSecondRemoval, Is.EqualTo(new[] { 2968 }), 
            "After removing 92, should only return [2968]");
            
        // Phase 3: Bulk removal (this is where AfterBulkRemove fails in validator)
        tree.Remove(new[] { 2968 });
        
        var afterBulkRemoval = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        Console.WriteLine($"After bulk removing 2968: [{string.Join(", ", afterBulkRemoval)}]");
        
        Assert.That(afterBulkRemoval, Is.Empty, "After removing all overlapping values, should be empty");
    }
    
    [Test]
    public void DeepDebugWithLogging_ValidatorSequence()
    {
        // Use the debug version to trace exactly what's happening
        var tree = new DebugRangeTreeAdapter<double, int>();
        
        Console.WriteLine("=== SETUP PHASE ===");
        tree.Add(4640.0, 4650.0, 92);
        tree.Add(4645.0, 4655.0, 2968);
        tree.Add(4635.0, 4645.0, 100);
        
        Console.WriteLine("=== INITIAL QUERY ===");
        var initialResult = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        
        Console.WriteLine("=== FIRST REMOVAL ===");
        tree.Remove(100);
        
        Console.WriteLine("=== QUERY AFTER FIRST REMOVAL ===");
        var afterFirstRemoval = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        
        Console.WriteLine("=== SECOND REMOVAL (THE BUG) ===");
        tree.Remove(92);
        
        Console.WriteLine("=== QUERY AFTER SECOND REMOVAL (SHOULD ONLY BE 2968) ===");
        var afterSecondRemoval = tree.Query(4644.875, 4652.362).OrderBy(x => x).ToArray();
        
        Console.WriteLine($"Final result: [{string.Join(", ", afterSecondRemoval)}]");
        
        // This should only contain [2968] but validator reports [92, 2968]
        Assert.That(afterSecondRemoval, Is.EqualTo(new[] { 2968 }), 
            "After removing 92, should only return [2968]");
    }
}