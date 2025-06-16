using System.Collections;
using System.Numerics;
using RangeFinder.Core;

namespace IntervalTree;

/// <summary>
/// RangeTree-compatible adapter around RangeFinder for drop-in replacement.
/// Provides the same API as RangeTree but with RangeFinder's optimized performance.
/// Original RangeTree: https://github.com/mbuchetics/RangeTree
/// </summary>
/// <typeparam name="TKey">The type of the interval key (must be numeric)</typeparam>
/// <typeparam name="TValue">The type of the associated value</typeparam>
public class RangeTreeAdapter<TKey, TValue> : IIntervalTree<TKey, TValue>
    where TKey : INumber<TKey>
{
    private readonly List<RangeValuePair<TKey, TValue>> _ranges;
    private RangeFinder<TKey, TValue>? _rangeFinder;
    private bool _isDirty = true;

    public RangeTreeAdapter()
    {
        _ranges = new List<RangeValuePair<TKey, TValue>>();
    }

    public RangeTreeAdapter(IEnumerable<RangeValuePair<TKey, TValue>> ranges) : this()
    {
        foreach (var range in ranges)
        {
            Add(range.From, range.To, range.Value);
        }
    }

    public IEnumerable<TValue> Values => _ranges.Select(r => r.Value);

    public int Count => _ranges.Count;

    public IEnumerable<TValue> Query(TKey value)
    {
        EnsureRangeFinderUpToDate();
        return _rangeFinder?.QueryRanges(value).Select(r => r.Value) ?? Enumerable.Empty<TValue>();
    }

    public IEnumerable<TValue> Query(TKey from, TKey to)
    {
        EnsureRangeFinderUpToDate();
        return _rangeFinder?.QueryRanges(from, to).Select(r => r.Value) ?? Enumerable.Empty<TValue>();
    }

    public void Add(TKey from, TKey to, TValue value)
    {
        _ranges.Add(new RangeValuePair<TKey, TValue>(from, to, value));
        _isDirty = true;
    }

    public void Remove(TValue item)
    {
        for (int i = _ranges.Count - 1; i >= 0; i--)
        {
            if (EqualityComparer<TValue>.Default.Equals(_ranges[i].Value, item))
            {
                _ranges.RemoveAt(i);
                _isDirty = true;
                // Continue to remove ALL occurrences, don't break
            }
        }
    }

    public void Remove(IEnumerable<TValue> items)
    {
        var itemsToRemove = new HashSet<TValue>(items);
        for (int i = _ranges.Count - 1; i >= 0; i--)
        {
            if (itemsToRemove.Contains(_ranges[i].Value))
            {
                _ranges.RemoveAt(i);
                _isDirty = true;
            }
        }
    }

    public void Clear()
    {
        _ranges.Clear();
        _rangeFinder = null;
        _isDirty = true;
    }

    public IEnumerator<RangeValuePair<TKey, TValue>> GetEnumerator()
    {
        return _ranges.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void EnsureRangeFinderUpToDate()
    {
        if (_isDirty || _rangeFinder == null)
        {
            if (_ranges.Count > 0)
            {
                var numericRanges = _ranges.Select(r => 
                    new NumericRange<TKey, TValue>(r.From, r.To, r.Value)).ToList();
                _rangeFinder = new RangeFinder<TKey, TValue>(numericRanges);
            }
            else
            {
                _rangeFinder = null;
            }
            _isDirty = false;
        }
    }
}

/// <summary>
/// Type alias for backward compatibility with original RangeTree naming.
/// Use RangeTreeAdapter for new code.
/// </summary>
/// <typeparam name="TKey">The type of the interval key (must be numeric)</typeparam>
/// <typeparam name="TValue">The type of the associated value</typeparam>
public class IntervalTree<TKey, TValue> : RangeTreeAdapter<TKey, TValue>
    where TKey : INumber<TKey>
{
    public IntervalTree() : base() { }
    public IntervalTree(IEnumerable<RangeValuePair<TKey, TValue>> ranges) : base(ranges) { }
}