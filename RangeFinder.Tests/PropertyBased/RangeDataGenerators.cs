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

        var rand = new System.Random(123);

        // Randomly select a query range from the provided data
        var valIndex1 = rand.Next(0, rangeData.Length);
        var valIndex2 = rand.Next(0, rangeData.Length);

        var val1 = rangeData[valIndex1].start * rand.NextDouble();
        var val2 = rangeData[valIndex2].end;
        return val1 <= val2 ? (val1, val2) : (val2, val1);
    }
}
