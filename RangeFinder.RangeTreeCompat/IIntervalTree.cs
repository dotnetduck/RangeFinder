using System.Collections;

namespace RangeFinder.RangeTreeCompat;

/// <summary>
/// Interface for interval tree implementations.
/// Original: https://github.com/mbuchetics/RangeTree
/// </summary>
/// <typeparam name="TKey">The type of the interval key</typeparam>
/// <typeparam name="TValue">The type of the associated value</typeparam>
public interface IIntervalTree<TKey, TValue> : IEnumerable<RangeValuePair<TKey, TValue>>
{
    /// <summary>
    /// Gets all values stored in the interval tree.
    /// </summary>
    IEnumerable<TValue> Values { get; }

    /// <summary>
    /// Gets the number of elements in the interval tree.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Queries for all values that overlap with the specified point.
    /// </summary>
    /// <param name="value">The point to query</param>
    /// <returns>All values that contain the specified point</returns>
    IEnumerable<TValue> Query(TKey value);

    /// <summary>
    /// Queries for all values that overlap with the specified range.
    /// </summary>
    /// <param name="from">The start of the query range</param>
    /// <param name="to">The end of the query range</param>
    /// <returns>All values that overlap with the specified range</returns>
    IEnumerable<TValue> Query(TKey from, TKey to);

    /// <summary>
    /// Adds a new interval with the specified range and value.
    /// </summary>
    /// <param name="from">The start of the interval</param>
    /// <param name="to">The end of the interval</param>
    /// <param name="value">The value to associate with the interval</param>
    void Add(TKey from, TKey to, TValue value);

    /// <summary>
    /// Removes the specified value from the tree.
    /// </summary>
    /// <param name="item">The value to remove</param>
    void Remove(TValue item);

    /// <summary>
    /// Removes multiple values from the tree.
    /// </summary>
    /// <param name="items">The values to remove</param>
    void Remove(IEnumerable<TValue> items);

    /// <summary>
    /// Removes all elements from the tree.
    /// </summary>
    void Clear();
}