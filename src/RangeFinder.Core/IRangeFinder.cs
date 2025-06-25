using System.Numerics;

namespace RangeFinder.Core;

/// <summary>
/// Provides an interface for efficiently finding overlapping ranges and ranges between
/// specified boundaries in a collection of NumericRange with optimized performance.
/// </summary>
/// <typeparam name="TNumber">The type of number used in the ranges.</typeparam>
/// <typeparam name="TAssociated">The type of value associated with each range.</typeparam>
public interface IRangeFinder<TNumber, TAssociated>
    where TNumber : INumber<TNumber>
{
    /// <summary>
    /// Gets the number of ranges in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets all ranges in the collection.
    /// </summary>
    IEnumerable<NumericRange<TNumber, TAssociated>> Values { get; }

    /// <summary>
    /// The minimum start value across all ranges.
    /// </summary>
    TNumber LowerBound { get; }

    /// <summary>
    /// The maximum end value across all ranges.
    /// </summary>
    TNumber UpperBound { get; }

    /// <summary>
    /// Finds all ranges that overlap with the specified range.
    /// Returns the complete NumericRange objects for detailed analysis.
    /// </summary>
    /// <param name="from">The start of the query range</param>
    /// <param name="to">The end of the query range</param>
    /// <returns>All ranges that overlap with the specified range</returns>
    IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber from, TNumber to);

    /// <summary>
    /// Finds all ranges that contain the specified point value.
    /// Returns the complete NumericRange objects for detailed analysis.
    /// </summary>
    /// <param name="value">The point value to search for</param>
    /// <returns>All ranges that contain the specified value</returns>
    IEnumerable<NumericRange<TNumber, TAssociated>> QueryRanges(TNumber value);
}
