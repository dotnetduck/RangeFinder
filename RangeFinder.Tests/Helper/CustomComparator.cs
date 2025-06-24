namespace RangeFinder.Tests.Helper;

/// <summary>
/// Custom assertions for property-based testing
/// </summary>
public static class CustomComparator
{
    /// <summary>
    /// Compares this sequence with another as sets and returns detailed difference information
    /// </summary>
    public static SetDifference<T> CompareAsSets<T>(this IEnumerable<T> actual, IEnumerable<T> expected) where T : notnull
    {
        var expectedSet = expected.ToHashSet();
        var actualSet = actual.ToHashSet();

        var onlyInExpected = expectedSet.Except(actualSet).ToHashSet();
        var onlyInActual = actualSet.Except(expectedSet).ToHashSet();

        return new SetDifference<T>(onlyInExpected, onlyInActual, actualSet.Count, expectedSet.Count);
    }
}
