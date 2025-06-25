using System.Numerics;

namespace RangeFinder.Core;

public class RangeFinder<TNumber, TAssociated> : IRangeFinder<TNumber, TAssociated>
    where TNumber : INumber<TNumber>
{
    public int Count => _sortedRanges.Length;
    public IEnumerable<NumericRange<TNumber, TAssociated>> Values => _sortedRanges;

    public TNumber LowerBound { get; init; }

    public TNumber UpperBound { get; init; }

    private readonly NumericRange<TNumber, TAssociated>[] _sortedRanges;
    private readonly bool[] _canTerminateHere; // Marks positions where no later range can overlap
    private readonly TNumber _maxSpanOfTheRangesForPruning;

    public RangeFinder(IEnumerable<NumericRange<TNumber, TAssociated>> ranges)
    {
        _sortedRanges = ranges.OrderBy(r => r.Start).ToArray();
        _canTerminateHere = new bool[_sortedRanges.Length];

        // Calculate max span for pruning
        if (_sortedRanges.Length > 0)
        {
            _maxSpanOfTheRangesForPruning = _sortedRanges.MaxBy(range => range.Span)!.Span;
        }
        else
        {
            _maxSpanOfTheRangesForPruning = TNumber.Zero;
        }

        // Pre-compute early termination markers and get bounds
        (LowerBound, UpperBound) = ComputeEarlyTerminationMarkers();
    }

    /// <summary>
    /// Pre-computes markers to identify positions where search can terminate early.
    /// For each position i, _canTerminateHere[i] is true if no range at position j > i
    /// can have an end time later than the start time of range at position i.
    /// Also computes and returns LowerBound and UpperBound during the same pass.
    /// </summary>
    private (TNumber LowerBound, TNumber UpperBound) ComputeEarlyTerminationMarkers()
    {
        if (_sortedRanges.Length == 0)
        {
            return (TNumber.Zero, TNumber.Zero);
        }

        // LowerBound is always the first element's start (array is sorted)
        var lowerBound = _sortedRanges[0].Start;

        // Work backwards to compute the maximum end time seen so far
        var maxEndSoFar = _sortedRanges[^1].End;
        _canTerminateHere[^1] = true; // Last element can always terminate

        for (var i = _sortedRanges.Length - 2; i >= 0; i--)
        {
            var currentRange = _sortedRanges[i];

            // If current range's start >= max end of all subsequent ranges,
            // we can terminate here when searching for overlaps
            if (currentRange.Start.CompareTo(maxEndSoFar) >= 0)
            {
                _canTerminateHere[i] = true;
            }

            // Update max end seen so far
            if (currentRange.End.CompareTo(maxEndSoFar) > 0)
            {
                maxEndSoFar = currentRange.End;
            }
        }

        // maxEndSoFar now contains the global maximum end value
        return (lowerBound, maxEndSoFar);
    }

    public IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber from, TNumber to)
    {
        var queryRange = new NumericRange<TNumber, TAssociated>(from, to);

        // Use bulk collection instead of yield return for better performance
        var results = new List<NumericRange<TNumber, TAssociated>>();

        // Start searching from the pruned range start
        var prunedRangeStart = queryRange.Start - _maxSpanOfTheRangesForPruning;

        // Binary search for starting position
        var startIndex = BinarySearchForStart(prunedRangeStart);

        // Linear scan with early termination
        for (var i = startIndex; i < _sortedRanges.Length; i++)
        {
            var range = _sortedRanges[i];

            // Early termination: if current range starts after query ends, we're done
            if (range.Start.CompareTo(queryRange.End) > 0)
            {
                break;
            }

            // Check for overlap (always include touching ranges)
            if (queryRange.Overlaps(range))
            {
                results.Add(range);
            }
        }

        return results;
    }

    public IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber value)
    {
        // Use bulk collection instead of yield return for better performance
        var results = new List<NumericRange<TNumber, TAssociated>>();

        // Start searching from the pruned range start
        var prunedRangeStart = value - _maxSpanOfTheRangesForPruning;

        // Binary search for starting position
        var startIndex = BinarySearchForStart(prunedRangeStart);

        // Linear scan with early termination
        for (var i = startIndex; i < _sortedRanges.Length; i++)
        {
            var range = _sortedRanges[i];

            // Early termination: if current range starts after the point, we're done
            if (range.Start.CompareTo(value) > 0)
            {
                break;
            }

            // Check if the point is within the range (inclusive of boundaries)
            if (range.Contains(value))
            {
                results.Add(range);
            }
        }

        return results;
    }



    /// <summary>
    /// Binary search to find the first range that could potentially overlap with the query.
    /// </summary>
    private int BinarySearchForStart(TNumber searchStart)
    {
        int left = 0, right = _sortedRanges.Length - 1;
        int result = _sortedRanges.Length; // Default to end if not found

        while (left <= right)
        {
            var mid = left + (right - left) / 2;

            if (_sortedRanges[mid].Start.CompareTo(searchStart) >= 0)
            {
                result = mid;
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        return result;
    }
}
