using System.Numerics;
using RangeFinder.Core;

namespace RangeFinder.Tests;

/// <summary>
/// Naive linear implementation of IRangeFinder for back-to-back testing.
/// This implementation prioritizes simplicity over performance for use as a reference
/// implementation in unit tests. It performs O(n) linear scans for all operations.
/// 
/// WARNING: This implementation is intentionally slow and should never be used in production.
/// It exists solely to provide a simple, obviously correct implementation for testing purposes.
/// </summary>
/// <typeparam name="TNumber">The type of number used in the ranges.</typeparam>
/// <typeparam name="TAssociated">The type of value associated with each range.</typeparam>
public class LinearRangeFinder<TNumber, TAssociated> : IRangeFinder<TNumber, TAssociated>
    where TNumber : INumber<TNumber>
{
    private readonly List<NumericRange<TNumber, TAssociated>> _ranges;

    public LinearRangeFinder(IEnumerable<NumericRange<TNumber, TAssociated>> ranges)
    {
        _ranges = ranges?.ToList() ?? throw new ArgumentNullException(nameof(ranges));
        
        // Calculate bounds
        if (_ranges.Count > 0)
        {
            LowerBound = _ranges.Min(r => r.Start)!;
            UpperBound = _ranges.Max(r => r.End)!;
        }
        else
        {
            LowerBound = TNumber.Zero;
            UpperBound = TNumber.Zero;
        }
    }

    public int Count => _ranges.Count;

    public IEnumerable<NumericRange<TNumber, TAssociated>> Values => _ranges;

    public TNumber LowerBound { get; init; }

    public TNumber UpperBound { get; init; }

    /// <summary>
    /// Naive O(n) implementation that checks every range for overlap.
    /// Simple and obviously correct for testing purposes.
    /// </summary>
    public IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber from, TNumber to)
    {
        NumericRange<TNumber, TAssociated> queryRange = new(from, to);
        List<NumericRange<TNumber, TAssociated>> results = [];

        // Linear scan through all ranges
        foreach (var range in _ranges)
        {
            if (queryRange.Overlaps(range))
            {
                results.Add(range);
            }
        }

        return results;
    }

    /// <summary>
    /// Naive O(n) implementation that checks every range for point containment.
    /// Simple and obviously correct for testing purposes.
    /// </summary>
    public IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber value)
    {
        List<NumericRange<TNumber, TAssociated>> results = [];

        // Linear scan through all ranges
        foreach (var range in _ranges)
        {
            // Check if the point is within the range (inclusive of boundaries)
            if (range.Start.CompareTo(value) <= 0 && value.CompareTo(range.End) <= 0)
            {
                results.Add(range);
            }
        }

        return results;
    }
}