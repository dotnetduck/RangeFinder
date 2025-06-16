using IntervalTree;
using RangeFinder.Core;

namespace RangeFinder.RangeTreeCompat.Tests;

/// <summary>
/// Basic functionality tests for the RangeTree compatibility wrapper.
/// </summary>
[TestFixture]
public class IntervalTreeBasicTests
{
    [Test]
    public void Constructor_EmptyTree_ShouldHaveZeroCount()
    {
        var tree = new IntervalTree<int, string>();
        
        Assert.That(tree.Count, Is.EqualTo(0));
        Assert.That(tree.Values, Is.Empty);
    }

    [Test]
    public void Add_SingleRange_ShouldIncreaseCount()
    {
        var tree = new IntervalTree<int, string>();
        
        tree.Add(1, 5, "A");
        
        Assert.That(tree.Count, Is.EqualTo(1));
        Assert.That(tree.Values, Contains.Item("A"));
    }

    [Test]
    public void Add_MultipleRanges_ShouldIncreaseCount()
    {
        var tree = new IntervalTree<int, string>();
        
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        tree.Add(10, 15, "C");
        
        Assert.That(tree.Count, Is.EqualTo(3));
        Assert.That(tree.Values, Is.EquivalentTo(new[] { "A", "B", "C" }));
    }

    [Test]
    public void Query_Point_ShouldReturnOverlappingRanges()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        tree.Add(10, 15, "C");
        
        var result = tree.Query(4).ToList();
        
        Assert.That(result, Is.EquivalentTo(new[] { "A", "B" }));
    }

    [Test]
    public void Query_Range_ShouldReturnOverlappingRanges()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        tree.Add(10, 15, "C");
        tree.Add(12, 20, "D");
        
        var result = tree.Query(6, 11).ToList();
        
        Assert.That(result, Is.EquivalentTo(new[] { "B", "C" }));
    }

    [Test]
    public void Query_EmptyTree_ShouldReturnEmpty()
    {
        var tree = new IntervalTree<int, string>();
        
        var pointResult = tree.Query(5).ToList();
        var rangeResult = tree.Query(1, 10).ToList();
        
        Assert.That(pointResult, Is.Empty);
        Assert.That(rangeResult, Is.Empty);
    }

    [Test]
    public void Query_NoOverlap_ShouldReturnEmpty()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(10, 15, "B");
        
        var result = tree.Query(7, 9).ToList();
        
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Remove_ExistingValue_ShouldDecreaseCount()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        tree.Add(10, 15, "C");
        
        tree.Remove("B");
        
        Assert.That(tree.Count, Is.EqualTo(2));
        Assert.That(tree.Values, Is.EquivalentTo(new[] { "A", "C" }));
        
        var result = tree.Query(4).ToList();
        Assert.That(result, Is.EquivalentTo(new[] { "A" }));
    }

    [Test]
    public void Remove_NonExistingValue_ShouldNotChangeCount()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        
        tree.Remove("Z");
        
        Assert.That(tree.Count, Is.EqualTo(2));
        Assert.That(tree.Values, Is.EquivalentTo(new[] { "A", "B" }));
    }

    [Test]
    public void Remove_Multiple_ShouldDecreaseCount()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        tree.Add(10, 15, "C");
        tree.Add(12, 20, "D");
        
        tree.Remove(new[] { "B", "D" });
        
        Assert.That(tree.Count, Is.EqualTo(2));
        Assert.That(tree.Values, Is.EquivalentTo(new[] { "A", "C" }));
    }

    [Test]
    public void Clear_ShouldEmptyTree()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        tree.Add(10, 15, "C");
        
        tree.Clear();
        
        Assert.That(tree.Count, Is.EqualTo(0));
        Assert.That(tree.Values, Is.Empty);
        Assert.That(tree.Query(5), Is.Empty);
    }

    [Test]
    public void Enumeration_ShouldReturnAllRangeValuePairs()
    {
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 5, "A");
        tree.Add(3, 7, "B");
        tree.Add(10, 15, "C");
        
        var pairs = tree.ToList();
        
        Assert.That(pairs.Count, Is.EqualTo(3));
        Assert.That(pairs.Select(p => p.Value), Is.EquivalentTo(new[] { "A", "B", "C" }));
        
        var pairA = pairs.First(p => p.Value == "A");
        Assert.That(pairA.From, Is.EqualTo(1));
        Assert.That(pairA.To, Is.EqualTo(5));
    }
}