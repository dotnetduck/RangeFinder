using System.Numerics;

namespace RangeFinder.Core;

/// <summary>
/// Extension methods providing IntervalTree-compatible API for RangeFinder.
/// These methods provide drop-in compatibility with IntervalTree library for easy migration.
/// </summary>
public static class IntervalTreeExtensions
{
    /// <summary>
    /// IntervalTree-compatible range query API. Returns only the associated values.
    /// This method provides drop-in compatibility with IntervalTree library for easy migration.
    /// Finds all values associated with ranges that overlap with the specified range.
    /// For accessing full range objects, use QueryRanges() instead.
    /// </summary>
    /// <param name="rangeFinder">The RangeFinder instance</param>
    /// <param name="from">The start of the query range</param>
    /// <param name="to">The end of the query range</param>
    /// <returns>All values associated with overlapping ranges (compatible with IntervalTree.Query)</returns>
    public static IEnumerable<TAssociated> Query<TNumber, TAssociated>(
        this RangeFinder<TNumber, TAssociated> rangeFinder, 
        TNumber from, 
        TNumber to)
        where TNumber : INumber<TNumber>
    {
        return rangeFinder.QueryRanges(from, to).Select(range => range.Value);
    }
    
    /// <summary>
    /// IntervalTree-compatible point query API. Returns only the associated values.
    /// This method provides drop-in compatibility with IntervalTree library for easy migration.
    /// Finds all values associated with ranges that contain the specified point value.
    /// For accessing full range objects, use QueryRanges() instead.
    /// </summary>
    /// <param name="rangeFinder">The RangeFinder instance</param>
    /// <param name="value">The point value to search for</param>
    /// <returns>All values associated with ranges that contain the specified value (compatible with IntervalTree.Query)</returns>
    public static IEnumerable<TAssociated> Query<TNumber, TAssociated>(
        this RangeFinder<TNumber, TAssociated> rangeFinder, 
        TNumber value)
        where TNumber : INumber<TNumber>
    {
        return rangeFinder.QueryRanges(value).Select(range => range.Value);
    }
}