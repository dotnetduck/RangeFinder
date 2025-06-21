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
}
