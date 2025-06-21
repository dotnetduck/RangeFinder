namespace RangeFinder.Tests;

/// <summary>
/// Custom assertions for property-based testing
/// </summary>
public static class CustomComparator
{
    /// <summary>
    /// Compares two sequences and returns detailed difference information
    /// </summary>
    public static SetDifference<T> CompareAsSets<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
    {
        var expectedSet = expected.ToHashSet();
        var actualSet = actual.ToHashSet();
        
        var onlyInExpected = expectedSet.Except(actualSet).ToHashSet();
        var onlyInActual = actualSet.Except(expectedSet).ToHashSet();
        
        return new SetDifference<T>(onlyInExpected, onlyInActual);
    }
}

/// <summary>
/// Contains difference information between two sets
/// </summary>
public record SetDifference<T>(HashSet<T> OnlyInExpected, HashSet<T> OnlyInActual)
{
    /// <summary>
    /// True if both sets are equal (no differences)
    /// </summary>
    public bool AreEqual => OnlyInExpected.Count == 0 && OnlyInActual.Count == 0;
    
    /// <summary>
    /// Creates a human-readable description of the differences
    /// </summary>
    public string GetDescription()
    {
        if (AreEqual) return "Sets are equal";
        
        var parts = new List<string>();
        if (OnlyInExpected.Count > 0)
            parts.Add($"Only in expected: [{string.Join(", ", OnlyInExpected)}]");
        if (OnlyInActual.Count > 0)
            parts.Add($"Only in actual: [{string.Join(", ", OnlyInActual)}]");
            
        return string.Join("; ", parts);
    }
}
