using IntervalTree;

namespace RangeFinder.RangeTreeCompat.Tests;

/// <summary>
/// Examples showing the new preferred naming and backward compatibility.
/// </summary>
[TestFixture]
public class NamingExampleTests
{
    [Test]
    public void PreferredNaming_RangeTreeAdapter_ShouldWork()
    {
        // Preferred: Use RangeTreeAdapter for new code
        var adapter = new RangeTreeAdapter<int, string>();
        adapter.Add(1, 10, "First");
        adapter.Add(5, 15, "Second");
        
        var results = adapter.Query(7).ToList();
        
        Assert.That(results, Is.EquivalentTo(new[] { "First", "Second" }));
        Assert.That(adapter.Count, Is.EqualTo(2));
    }
    
    [Test]
    public void BackwardCompatibility_IntervalTree_ShouldWork()
    {
        // Backward compatible: IntervalTree still works for existing code
        var tree = new IntervalTree<int, string>();
        tree.Add(1, 10, "First");
        tree.Add(5, 15, "Second");
        
        var results = tree.Query(7).ToList();
        
        Assert.That(results, Is.EquivalentTo(new[] { "First", "Second" }));
        Assert.That(tree.Count, Is.EqualTo(2));
    }
    
    [Test]
    public void BothNamesAreEquivalent_ShouldBehaveIdentically()
    {
        var adapter = new RangeTreeAdapter<double, int>();
        var tree = new IntervalTree<double, int>();
        
        // Add same data to both
        adapter.Add(1.0, 5.0, 100);
        adapter.Add(3.0, 7.0, 200);
        
        tree.Add(1.0, 5.0, 100);
        tree.Add(3.0, 7.0, 200);
        
        // Both should return same results
        var adapterResults = adapter.Query(4.0).OrderBy(x => x).ToArray();
        var treeResults = tree.Query(4.0).OrderBy(x => x).ToArray();
        
        Assert.That(adapterResults, Is.EqualTo(treeResults));
        Assert.That(adapter.Count, Is.EqualTo(tree.Count));
        Assert.That(adapter.Values.OrderBy(x => x), Is.EqualTo(tree.Values.OrderBy(x => x)));
    }
}