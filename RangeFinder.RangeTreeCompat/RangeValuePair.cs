namespace RangeFinder.RangeTreeCompat;

/// <summary>
/// Represents a range-value pair for RangeTree compatibility.
/// Original: https://github.com/mbuchetics/RangeTree
/// </summary>
/// <typeparam name="TKey">The type of the range key</typeparam>
/// <typeparam name="TValue">The type of the associated value</typeparam>
public class RangeValuePair<TKey, TValue>
{
    public TKey From { get; set; }
    public TKey To { get; set; }
    public TValue Value { get; set; }

    public RangeValuePair(TKey from, TKey to, TValue value)
    {
        From = from;
        To = to;
        Value = value;
    }
}