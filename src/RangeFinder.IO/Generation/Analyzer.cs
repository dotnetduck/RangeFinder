using RangeFinder.Core;
using System.Numerics;

namespace RangeFinder.IO.Generation;

public static class Analyzer
{
    /// <summary>
    /// Analyzes statistical characteristics of dataset
    /// </summary>
    public static Stats Analyze<TNumber>(IEnumerable<NumericRange<TNumber, int>> ranges)
        where TNumber : INumber<TNumber>
    {
        var rangeList = ranges.ToList();
        if (rangeList.Count == 0)
        {
            return new Stats(0, 0, 0, 0, 0, 0, 0, 0);
        }

        var lengths = rangeList.Select(r => Convert.ToDouble(r.End - r.Start)).ToArray();
        var starts = rangeList.Select(r => Convert.ToDouble(r.Start)).ToArray();
        var ends = rangeList.Select(r => Convert.ToDouble(r.End)).ToArray();
        var sortedStarts = starts.OrderBy(s => s).ToArray();
        var intervals = sortedStarts.Skip(1).Zip(sortedStarts, (next, curr) => next - curr).ToArray();

        var avgLength = lengths.Average();
        var stdDevLength = lengths.Length > 1 ?
            Math.Sqrt(lengths.Select(l => Math.Pow(l - avgLength, 2)).Average()) : 0;

        var avgInterval = intervals.Length > 0 ? intervals.Average() : 0;
        var stdDevInterval = intervals.Length > 1 ?
            Math.Sqrt(intervals.Select(i => Math.Pow(i - avgInterval, 2)).Average()) : 0;

        var overlapPercentage = CalculateOverlapPercentageOptimized(rangeList);

        return new Stats(
            rangeList.Count,
            starts.Min(),
            ends.Max(),
            avgLength,
            stdDevLength,
            avgInterval,
            stdDevInterval,
            overlapPercentage
        );
    }

    /// <summary>
    /// Calculate overlap percentage as average coverage depth across the occupied space
    /// </summary>
    private static double CalculateOverlapPercentageOptimized<TNumber>(List<NumericRange<TNumber, int>> ranges)
        where TNumber : INumber<TNumber>
    {
        if (ranges.Count <= 1)
        {
            return 0;
        }

        // Use sweep line algorithm to calculate coverage depth
        var events = new List<(double Position, int Delta)>();

        foreach (var range in ranges)
        {
            var start = Convert.ToDouble(range.Start);
            var end = Convert.ToDouble(range.End);
            events.Add((start, 1));     // Range starts: +1 coverage
            events.Add((end, -1));      // Range ends: -1 coverage
        }

        // Sort events by position, with ends processed before starts at same position
        events.Sort((a, b) => a.Position != b.Position ?
            a.Position.CompareTo(b.Position) :
            a.Delta.CompareTo(b.Delta));

        double totalWeightedCoverage = 0;
        double totalLength = 0;
        int currentDepth = 0;
        double lastPosition = events[0].Position;

        foreach (var (position, delta) in events)
        {
            if (currentDepth > 0)
            {
                var segmentLength = position - lastPosition;
                totalWeightedCoverage += segmentLength * currentDepth;
                totalLength += segmentLength;
            }

            currentDepth += delta;
            lastPosition = position;
        }

        if (totalLength == 0)
        {
            return 0;
        }

        // Average coverage depth - 1.0 means no overlap, 2.0 means double coverage on average
        var averageDepth = totalWeightedCoverage / totalLength;

        // Convert to overlap percentage: depth > 1.0 indicates overlap
        return Math.Max(0, (averageDepth - 1.0) * 100);
    }
}
