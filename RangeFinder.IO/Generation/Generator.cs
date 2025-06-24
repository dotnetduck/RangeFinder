using RangeFinder.Core;
using System.Numerics;

namespace RangeFinder.IO.Generation;

/// <summary>
/// Parameterized range generator for systematic performance testing.
/// Creates ranges with diverse data characteristics and controlled statistical properties
/// to validate RangeFinder performance.
/// </summary>
public static class Generator
{

    /// <summary>
    /// Generates ranges according to the specified parameters.
    /// </summary>
    public static List<NumericRange<TNumber, int>> GenerateRanges<TNumber>(Parameter parameters)
        where TNumber : INumber<TNumber>
    {
        var lowLevelParams = ParameterConverter.ToLowLevel(parameters);
        return InternalRangeGenerator.Generate<TNumber>(lowLevelParams);
    }

    /// <summary>
    /// Generates query ranges for testing against the dataset.
    /// </summary>
    public static List<NumericRange<TNumber, object>> GenerateQueryRanges<TNumber>(
        Parameter datasetParams,
        int queryCount,
        double queryLengthMultiplier = 2.0)
        where TNumber : INumber<TNumber>
    {
        var lowLevelParams = ParameterConverter.ToLowLevel(datasetParams);
        return QueryGenerator.GenerateRangeQueries<TNumber>(lowLevelParams, queryCount, queryLengthMultiplier);
    }

    /// <summary>
    /// Generates point queries for testing against the dataset.
    /// </summary>
    public static List<TNumber> GenerateQueryPoints<TNumber>(
        Parameter datasetParams,
        int queryCount)
        where TNumber : INumber<TNumber>
    {
        var lowLevelParams = ParameterConverter.ToLowLevel(datasetParams);
        return QueryGenerator.GeneratePointQueries<TNumber>(lowLevelParams, queryCount);
    }

    /// <summary>
    /// Analyzes the characteristics of the generated dataset for verification.
    /// </summary>
    public static Stats AnalyzeDataset<TNumber>(IEnumerable<NumericRange<TNumber, int>> ranges)
        where TNumber : INumber<TNumber>
    {
        return Analyzer.Analyze(ranges);
    }
}

/// <summary>
/// Internal component that converts high-level parameters to low-level parameters
/// </summary>
internal static class ParameterConverter
{
    /// <summary>
    /// Converts high-level parameters to low-level parameters
    /// </summary>
    public static LowLevelParameter ToLowLevel(Parameter parameters)
    {
        parameters.Validate();

        var totalSpace = parameters.TotalSpace;
        var rangeMinValue = totalSpace * parameters.StartOffset;
        var rangeMaxValue = totalSpace * (parameters.StartOffset + 1.0);
        var averageLength = parameters.AverageLength;
        var lengthStdDev = averageLength * parameters.LengthVariability;

        // Calculate interval from overlap depth
        var currentOverlapDepth = (parameters.Count * averageLength) / totalSpace;
        var averageInterval = CalculateInterval(parameters, currentOverlapDepth, averageLength);
        var intervalStdDev = averageInterval * parameters.ClusteringFactor;

        return new LowLevelParameter(
            RangeMinValue: rangeMinValue,
            RangeMaxValue: rangeMaxValue,
            RangeLengthAverage: averageLength,
            RangeLengthStdDev: lengthStdDev,
            RangeIntervalAverage: averageInterval,
            RangeIntervalStdDev: intervalStdDev,
            Count: parameters.Count,
            RandomSeed: parameters.RandomSeed
        );
    }

    private static double CalculateInterval(Parameter parameters, double currentOverlapDepth, double averageLength)
    {
        // For overlap depth D with average length L and interval I:
        // D = L / I (when I < L, ranges overlap)
        // Therefore: I = L / D

        var targetInterval = averageLength / parameters.OverlapFactor;

        // Ensure minimum interval to prevent infinite loops
        return Math.Max(0.001, targetInterval);
    }
}

/// <summary>
/// Internal component responsible for actual range generation
/// </summary>
internal static class InternalRangeGenerator
{
    /// <summary>
    /// Generates ranges from low-level parameters
    /// </summary>
    public static List<NumericRange<TNumber, int>> Generate<TNumber>(LowLevelParameter parameters)
        where TNumber : INumber<TNumber>
    {
        var random = new Random(parameters.RandomSeed);
        var normalGenerator = new NormalGenerator();
        var ranges = new List<NumericRange<TNumber, int>>(parameters.Count);

        var currentPosition = parameters.RangeMinValue;
        var spaceSize = parameters.RangeMaxValue - parameters.RangeMinValue;

        for (int i = 0; i < parameters.Count; i++)
        {
            // Generate range length using normal distribution
            var rangeLength = Math.Max(0.001,
                normalGenerator.Sample(random, parameters.RangeLengthAverage, parameters.RangeLengthStdDev));

            // Generate interval to next range using normal distribution
            var interval = Math.Max(0.0,
                normalGenerator.Sample(random, parameters.RangeIntervalAverage, parameters.RangeIntervalStdDev));

            // Calculate range boundaries with more robust boundary checking
            var availableSpace = parameters.RangeMaxValue - currentPosition;
            var actualLength = Math.Min(rangeLength, Math.Max(0.001, availableSpace));

            var start = Math.Clamp(currentPosition, parameters.RangeMinValue, parameters.RangeMaxValue - actualLength);
            var end = Math.Clamp(start + actualLength, start + 0.001, parameters.RangeMaxValue);

            // Final boundary check
            start = Math.Clamp(start, parameters.RangeMinValue, end - 0.001);

            try
            {
                // Overflow protection for numeric conversion
                var startValue = TNumber.CreateChecked(start);
                var endValue = TNumber.CreateChecked(end);
                ranges.Add(new NumericRange<TNumber, int>(startValue, endValue, i));
            }
            catch (OverflowException ex)
            {
                throw new InvalidOperationException(
                    $"Generated range values exceed the capacity of {typeof(TNumber).Name}", ex);
            }

            // Move to next position using interval between range starts
            currentPosition += interval;

            // Better wrap-around when exceeding space
            if (currentPosition >= parameters.RangeMaxValue)
            {
                var remainingRanges = parameters.Count - i - 1;
                if (remainingRanges > 0)
                {
                    var usedSpace = spaceSize * 0.1;
                    currentPosition = parameters.RangeMinValue +
                        random.NextDouble() * Math.Min(usedSpace, spaceSize * 0.3);
                }
            }
        }

        return ranges;
    }
}

/// <summary>
/// Internal component responsible for query generation
/// </summary>
internal static class QueryGenerator
{
    /// <summary>
    /// Generates range queries
    /// </summary>
    public static List<NumericRange<TNumber, object>> GenerateRangeQueries<TNumber>(
        LowLevelParameter datasetParams,
        int queryCount,
        double queryLengthMultiplier)
        where TNumber : INumber<TNumber>
    {
        var random = new Random(datasetParams.RandomSeed + 1000);
        var normalGenerator = new NormalGenerator();
        var queries = new List<NumericRange<TNumber, object>>(queryCount);

        var queryLengthAverage = datasetParams.RangeLengthAverage * queryLengthMultiplier;
        var queryLengthStdDev = datasetParams.RangeLengthStdDev * queryLengthMultiplier;
        var spaceSize = datasetParams.RangeMaxValue - datasetParams.RangeMinValue;

        for (int i = 0; i < queryCount; i++)
        {
            var queryStart = random.NextDouble() * spaceSize + datasetParams.RangeMinValue;
            var queryLength = Math.Max(0.001,
                normalGenerator.Sample(random, queryLengthAverage, queryLengthStdDev));

            var queryEnd = Math.Min(queryStart + queryLength, datasetParams.RangeMaxValue);
            queryStart = Math.Max(queryStart, datasetParams.RangeMinValue);

            try
            {
                var startValue = TNumber.CreateChecked(queryStart);
                var endValue = TNumber.CreateChecked(queryEnd);
                queries.Add(new NumericRange<TNumber, object>(startValue, endValue));
            }
            catch (OverflowException ex)
            {
                throw new InvalidOperationException(
                    $"Generated query values exceed the capacity of {typeof(TNumber).Name}", ex);
            }
        }

        return queries;
    }

    /// <summary>
    /// Generates point queries
    /// </summary>
    public static List<TNumber> GeneratePointQueries<TNumber>(
        LowLevelParameter datasetParams,
        int queryCount)
        where TNumber : INumber<TNumber>
    {
        var random = new Random(datasetParams.RandomSeed + 2000);
        var points = new List<TNumber>(queryCount);
        var spaceSize = datasetParams.RangeMaxValue - datasetParams.RangeMinValue;

        for (int i = 0; i < queryCount; i++)
        {
            var point = random.NextDouble() * spaceSize + datasetParams.RangeMinValue;

            try
            {
                points.Add(TNumber.CreateChecked(point));
            }
            catch (OverflowException ex)
            {
                throw new InvalidOperationException(
                    $"Generated point values exceed the capacity of {typeof(TNumber).Name}", ex);
            }
        }

        return points;
    }
}

/// <summary>
/// Internal low-level parameters
/// </summary>
internal record LowLevelParameter(
    double RangeMinValue,
    double RangeMaxValue,
    double RangeLengthAverage,
    double RangeLengthStdDev,
    double RangeIntervalAverage,
    double RangeIntervalStdDev,
    int Count,
    int RandomSeed = 42
);

/// <summary>
/// Efficient normal distribution generator using Box-Muller transformation
/// </summary>
internal class NormalGenerator
{
    private double? _spare;

    public double Sample(Random random, double mean, double stdDev)
    {
        if (_spare.HasValue)
        {
            var result = _spare.Value;
            _spare = null;
            return mean + stdDev * result;
        }

        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var mag = Math.Sqrt(-2.0 * Math.Log(u1));
        _spare = mag * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * mag * Math.Sin(2.0 * Math.PI * u2);
    }
}
