using FsCheck;

namespace RangeFinder.PropertyBased.Tests;

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
        {
            return (0.0, 0.0);
        }

        var rand = new System.Random();

        // Select a random range from the data to ensure overlap
        var selectedRange = rangeData[rand.Next(rangeData.Length)];

        // Generate a point within this range to ensure intersection
        var point = selectedRange.start + (selectedRange.end - selectedRange.start) * rand.NextDouble();

        // Create a query range around this point
        var rangeSize = (selectedRange.end - selectedRange.start) * (0.5 + rand.NextDouble() * 0.5);
        var queryStart = point - rangeSize * rand.NextDouble();
        var queryEnd = point + rangeSize * rand.NextDouble();

        return (queryStart, queryEnd);
    }

    public static double ExtractQueryPoint((double start, double end)[] rangeData)
    {
        if (rangeData.Length == 0)
        {
            return 0.0;
        }

        var rand = new System.Random();

        // Select a random range from the data
        var selectedRange = rangeData[rand.Next(rangeData.Length)];

        // Generate a point within this range to ensure intersection
        return selectedRange.start + (selectedRange.end - selectedRange.start) * rand.NextDouble();
    }
}
