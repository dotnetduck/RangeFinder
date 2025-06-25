using System;
using System.Numerics;

namespace RangeFinder.Core;

public static class RangeFinderFactory
{
    /// <summary>
    /// Creates a RangeFinder instance from a collection of NumericRange objects.
    /// Type parameters will be inferred from the provided ranges.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <param name="ranges">Collection of ranges to index</param>
    /// <returns>A new RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when ranges is null</exception>
    public static RangeFinder<TNumber, TAssociated>
    Create<TNumber, TAssociated>(IEnumerable<NumericRange<TNumber, TAssociated>> ranges)
        where TNumber : INumber<TNumber>
    {
        if (ranges == null)
        {
            throw new ArgumentNullException(nameof(ranges), "Ranges collection cannot be null.");
        }

        return new RangeFinder<TNumber, TAssociated>(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder from a collection of value tuples representing ranges.
    /// Convenient method for creating ranges from simple (start, end, value) tuples.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <param name="ranges">Collection of (start, end, value) tuples</param>
    /// <returns>A new RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when ranges is null</exception>
    public static RangeFinder<TNumber, TAssociated>
    Create<TNumber, TAssociated>(IEnumerable<(TNumber Start, TNumber End, TAssociated Value)> ranges)
        where TNumber : INumber<TNumber>
    {
        if (ranges == null)
        {
            throw new ArgumentNullException(nameof(ranges), "Ranges collection cannot be null.");
        }

        var numericRanges = ranges.Select(r => new NumericRange<TNumber, TAssociated>(r.Start, r.End, r.Value));
        return new RangeFinder<TNumber, TAssociated>(numericRanges);
    }

    /// <summary>
    /// Creates a RangeFinder for ranges without associated values (using range index as value).
    /// Useful when you only need to know which ranges overlap, not their associated data.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <param name="ranges">Collection of (start, end) tuples</param>
    /// <returns>A new RangeFinder instance with int indices as associated values</returns>
    /// <exception cref="ArgumentNullException">Thrown when ranges is null</exception>
    public static RangeFinder<TNumber, int>
    Create<TNumber>(IEnumerable<(TNumber Start, TNumber End)> ranges)
        where TNumber : INumber<TNumber>
    {
        if (ranges == null)
        {
            throw new ArgumentNullException(nameof(ranges), "Ranges collection cannot be null.");
        }

        var numericRanges = ranges.Select((r, index) => new NumericRange<TNumber, int>(r.Start, r.End, index));
        return new RangeFinder<TNumber, int>(numericRanges);
    }

    /// <summary>
    /// Creates a RangeFinder from separate start, end, and value arrays.
    /// All arrays must have the same length.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <param name="starts">Array of start values</param>
    /// <param name="ends">Array of end values</param>
    /// <param name="values">Array of associated values</param>
    /// <returns>A new RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when any array is null</exception>
    /// <exception cref="ArgumentException">Thrown when arrays have different lengths</exception>
    public static RangeFinder<TNumber, TAssociated>
    Create<TNumber, TAssociated>(TNumber[] starts, TNumber[] ends, TAssociated[] values)
        where TNumber : INumber<TNumber>
    {
        if (starts == null)
        {
            throw new ArgumentNullException(nameof(starts), "Starts array cannot be null.");
        }

        if (ends == null)
        {
            throw new ArgumentNullException(nameof(ends), "Ends array cannot be null.");
        }

        if (values == null)
        {
            throw new ArgumentNullException(nameof(values), "Values array cannot be null.");
        }

        if (starts.Length != ends.Length || starts.Length != values.Length)
        {
            throw new ArgumentException("All arrays must have the same length.");
        }

        var ranges = starts.Zip(ends, values)
            .Select(tuple => new NumericRange<TNumber, TAssociated>(tuple.First, tuple.Second, tuple.Third));

        return new RangeFinder<TNumber, TAssociated>(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder from separate start and end arrays with indices as values.
    /// Both arrays must have the same length.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <param name="starts">Array of start values</param>
    /// <param name="ends">Array of end values</param>
    /// <returns>A new RangeFinder instance with int indices as associated values</returns>
    /// <exception cref="ArgumentNullException">Thrown when any array is null</exception>
    /// <exception cref="ArgumentException">Thrown when arrays have different lengths</exception>
    public static RangeFinder<TNumber, int>
    Create<TNumber>(TNumber[] starts, TNumber[] ends)
        where TNumber : INumber<TNumber>
    {
        if (starts == null)
        {
            throw new ArgumentNullException(nameof(starts), "Starts array cannot be null.");
        }

        if (ends == null)
        {
            throw new ArgumentNullException(nameof(ends), "Ends array cannot be null.");
        }

        if (starts.Length != ends.Length)
        {
            throw new ArgumentException("Start and end arrays must have the same length.");
        }

        var ranges = starts.Zip(ends)
            .Select((tuple, index) => new NumericRange<TNumber, int>(tuple.First, tuple.Second, index));

        return new RangeFinder<TNumber, int>(ranges);
    }

    /// <summary>
    /// Creates an empty RangeFinder instance for the specified types.
    /// Might be useful for initialization when ranges will be added later or for edge cases.
    /// Note that dynamic building of ranges is not supported so far.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <returns>An empty RangeFinder instance</returns>
    public static RangeFinder<TNumber, TAssociated>
    CreateEmpty<TNumber, TAssociated>()
        where TNumber : INumber<TNumber>
    {
        return new RangeFinder<TNumber, TAssociated>([]);
    }
}
