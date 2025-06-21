using FsCheck;

namespace RangeFinder.Tests.PropertyBased;

/// <summary>
/// Custom generators for FsCheck property-based tests
/// </summary>
public static class RangeDataGenerators
{
    /// <summary>
    /// Generates valid range tuples ensuring start <= end
    /// </summary>
    public static Arbitrary<(double start, double end)> ValidRangeTuple()
    {
        var gen = Gen.Choose(-10000, 10000)
            .Two()
            .Select(pair =>
            {
                var start = pair.Item1 / 10.0;
                var end = pair.Item2 / 10.0;
                return start <= end ? (start, end) : (end, start);
            });

        return gen.ToArbitrary();
    }

    public static (double start, double end) ExtractQueryRange((double start, double end)[] rangeData)
    {
        if (rangeData.Length == 0)
            return (0.0, 0.0);

        var rand = new System.Random();

        // Generate a query range that intersects with some of the data
        var minStart = rangeData.Min(r => r.start);
        var maxEnd = rangeData.Max(r => r.end);
        
        // Create a query range within the bounds of the data
        var queryStart = minStart + (maxEnd - minStart) * rand.NextDouble() * 0.8;
        var queryEnd = queryStart + (maxEnd - queryStart) * rand.NextDouble();
        
        return (queryStart, queryEnd);
    }
}
