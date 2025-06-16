using System.Collections;
using System.Numerics;
using RangeFinder.Core;
using IntervalTree;

namespace RangeFinder.RangeTreeCompat.Tests;

/// <summary>
/// Debug version of RangeTreeAdapter with extensive logging to track down the validator bug.
/// </summary>
public class DebugRangeTreeAdapter<TKey, TValue> : IIntervalTree<TKey, TValue>
    where TKey : INumber<TKey>
{
    private readonly List<RangeValuePair<TKey, TValue>> _ranges;
    private RangeFinder<TKey, TValue>? _rangeFinder;
    private bool _isDirty = true;

    public DebugRangeTreeAdapter()
    {
        _ranges = new List<RangeValuePair<TKey, TValue>>();
        Console.WriteLine($"[DEBUG] DebugRangeTreeAdapter created");
    }

    public IEnumerable<TValue> Values => _ranges.Select(r => r.Value);

    public int Count => _ranges.Count;

    public IEnumerable<TValue> Query(TKey value)
    {
        Console.WriteLine($"[DEBUG] Query({value}) called, _isDirty={_isDirty}, _ranges.Count={_ranges.Count}");
        EnsureRangeFinderUpToDate();
        var result = _rangeFinder?.QueryRanges(value).Select(r => r.Value) ?? Enumerable.Empty<TValue>();
        var resultArray = result.ToArray();
        Console.WriteLine($"[DEBUG] Query({value}) returning [{string.Join(", ", resultArray)}]");
        return resultArray;
    }

    public IEnumerable<TValue> Query(TKey from, TKey to)
    {
        Console.WriteLine($"[DEBUG] Query({from}, {to}) called, _isDirty={_isDirty}, _ranges.Count={_ranges.Count}");
        Console.WriteLine($"[DEBUG] Current _ranges: [{string.Join(", ", _ranges.Select(r => $"({r.From}-{r.To}:{r.Value})"))}]");
        EnsureRangeFinderUpToDate();
        var result = _rangeFinder?.QueryRanges(from, to).Select(r => r.Value) ?? Enumerable.Empty<TValue>();
        var resultArray = result.ToArray();
        Console.WriteLine($"[DEBUG] Query({from}, {to}) returning [{string.Join(", ", resultArray)}]");
        return resultArray;
    }

    public void Add(TKey from, TKey to, TValue value)
    {
        Console.WriteLine($"[DEBUG] Add({from}, {to}, {value}) called");
        _ranges.Add(new RangeValuePair<TKey, TValue>(from, to, value));
        _isDirty = true;
        Console.WriteLine($"[DEBUG] After Add: _ranges.Count={_ranges.Count}, _isDirty={_isDirty}");
    }

    public void Remove(TValue item)
    {
        Console.WriteLine($"[DEBUG] Remove({item}) called");
        Console.WriteLine($"[DEBUG] Before Remove: _ranges=[{string.Join(", ", _ranges.Select(r => $"({r.From}-{r.To}:{r.Value})"))}]");
        
        for (int i = _ranges.Count - 1; i >= 0; i--)
        {
            if (EqualityComparer<TValue>.Default.Equals(_ranges[i].Value, item))
            {
                Console.WriteLine($"[DEBUG] Removing range at index {i}: ({_ranges[i].From}-{_ranges[i].To}:{_ranges[i].Value})");
                _ranges.RemoveAt(i);
                _isDirty = true;
                break;
            }
        }
        
        Console.WriteLine($"[DEBUG] After Remove: _ranges.Count={_ranges.Count}, _isDirty={_isDirty}");
        Console.WriteLine($"[DEBUG] After Remove: _ranges=[{string.Join(", ", _ranges.Select(r => $"({r.From}-{r.To}:{r.Value})"))}]");
    }

    public void Remove(IEnumerable<TValue> items)
    {
        var itemsToRemove = new HashSet<TValue>(items);
        Console.WriteLine($"[DEBUG] Remove(bulk) called with [{string.Join(", ", itemsToRemove)}]");
        Console.WriteLine($"[DEBUG] Before bulk Remove: _ranges=[{string.Join(", ", _ranges.Select(r => $"({r.From}-{r.To}:{r.Value})"))}]");
        
        for (int i = _ranges.Count - 1; i >= 0; i--)
        {
            if (itemsToRemove.Contains(_ranges[i].Value))
            {
                Console.WriteLine($"[DEBUG] Bulk removing range at index {i}: ({_ranges[i].From}-{_ranges[i].To}:{_ranges[i].Value})");
                _ranges.RemoveAt(i);
                _isDirty = true;
            }
        }
        
        Console.WriteLine($"[DEBUG] After bulk Remove: _ranges.Count={_ranges.Count}, _isDirty={_isDirty}");
        Console.WriteLine($"[DEBUG] After bulk Remove: _ranges=[{string.Join(", ", _ranges.Select(r => $"({r.From}-{r.To}:{r.Value})"))}]");
    }

    public void Clear()
    {
        Console.WriteLine($"[DEBUG] Clear() called");
        _ranges.Clear();
        _rangeFinder = null;
        _isDirty = true;
        Console.WriteLine($"[DEBUG] After Clear: _ranges.Count={_ranges.Count}, _isDirty={_isDirty}");
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
        Console.WriteLine($"[DEBUG] EnsureRangeFinderUpToDate called: _isDirty={_isDirty}, _rangeFinder==null={_rangeFinder == null}");
        
        if (_isDirty || _rangeFinder == null)
        {
            Console.WriteLine($"[DEBUG] Reconstructing RangeFinder with {_ranges.Count} ranges");
            
            if (_ranges.Count > 0)
            {
                var numericRanges = _ranges.Select(r => 
                    new NumericRange<TKey, TValue>(r.From, r.To, r.Value)).ToList();
                Console.WriteLine($"[DEBUG] Creating RangeFinder with ranges: [{string.Join(", ", numericRanges.Select(r => $"({r.Start}-{r.End}:{r.Value})"))}]");
                _rangeFinder = new RangeFinder<TKey, TValue>(numericRanges);
            }
            else
            {
                Console.WriteLine($"[DEBUG] No ranges, setting _rangeFinder to null");
                _rangeFinder = null;
            }
            _isDirty = false;
            Console.WriteLine($"[DEBUG] RangeFinder reconstruction complete, _isDirty={_isDirty}");
        }
        else
        {
            Console.WriteLine($"[DEBUG] RangeFinder is up to date, no reconstruction needed");
        }
    }
}