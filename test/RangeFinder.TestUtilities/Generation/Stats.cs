namespace RangeFinder.TestUtilities.Generation;

public record Stats(
    int Count,
    double MinValue,
    double MaxValue,
    double AverageLength,
    double LengthStdDev,
    double AverageInterval,
    double IntervalStdDev,
    double OverlapPercentage
)
{
    /// <summary>
    /// Returns formatted string describing dataset characteristics
    /// </summary>
    public string FormatCharacteristics()
    {
        var spread = MaxValue - MinValue;

        return $"Overlap: {OverlapPercentage:F1}%, " +
                $"Avg Length: {AverageLength:F2}, " +
                $"Spread: {spread:F2}, " +
                $"Avg Interval: {AverageInterval:F2}";
    }
};