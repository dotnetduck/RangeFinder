using System.ComponentModel;
using System.Numerics;

namespace RangeFinder.Core;

public record NumericRange<TRangeNumber, TAssociated>(
    TRangeNumber Start,
    TRangeNumber End,
    TAssociated Value = default!)
    : IComparable<NumericRange<TRangeNumber, TAssociated>>
    where TRangeNumber : INumber<TRangeNumber>
{
    /// <summary>
    /// Parameterless constructor for serialization frameworks that require it
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public NumericRange() : this(default!, default!, default!) { }

    public TRangeNumber Span => End - Start;

    /// <summary>
    /// Determines if this range overlaps with another range (including touching boundaries)
    /// </summary>
    public bool Overlaps(NumericRange<TRangeNumber, TAssociated> other) =>
        other.Start <= End && Start <= other.End;

    /// <summary>
    /// Determines if this range overlaps with a query range specified by start and end values
    /// </summary>
    public bool Overlaps(TRangeNumber queryStart, TRangeNumber queryEnd) =>
        queryStart <= End && Start <= queryEnd;

    #region For backwards compatibility
    [Obsolete("This method will be removed in future versions. Use Overlaps() instead")]
    public bool OverlapsIncludeTouching(NumericRange<TRangeNumber, TAssociated> queryNumericRange) =>
        Overlaps(queryNumericRange);

    [Obsolete("This method will be removed in future versions as RangeFinder always includes touching boundaries")]
    public bool OverlapsExceptTouching(NumericRange<TRangeNumber, TAssociated> queryNumericRange) =>
        queryNumericRange.Start < End && Start < queryNumericRange.End;
    #endregion

    public int CompareTo(NumericRange<TRangeNumber, TAssociated>? other)
    {
        if (other is null)
        {
            return 1;
        }

        var startComparison = Start.CompareTo(other.Start);
        return startComparison != 0
            ? startComparison
            : End.CompareTo(other.End);
    }
}
