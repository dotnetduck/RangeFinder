namespace RangeFinder.Tests.PropertyBased;

/// <summary>
/// Custom assertions for property-based testing
/// </summary>
public static class CustomAssert
{
    /// <summary>
    /// Checks if two sequences contain the same elements regardless of order
    /// </summary>
    public static bool SetEquals<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        var expectedSet = expected.ToHashSet();
        var actualSet = actual.ToHashSet();
        return expectedSet.SetEquals(actualSet);
    }
}
