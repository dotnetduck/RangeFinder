using System.Numerics;

namespace RangeFinder.Core;

public record NumericRange<TRangeNumber, TAssociated>(
    TRangeNumber Start, 
    TRangeNumber End, 
    TAssociated Value = default!)
    : IComparable<NumericRange<TRangeNumber, TAssociated>>
    where TRangeNumber : INumber<TRangeNumber>
{
    public TRangeNumber Span => End - Start;

    public bool OverlapsIncludeTouching(NumericRange<TRangeNumber, TAssociated> queryNumericRange) =>
        queryNumericRange.Start <= End && Start <= queryNumericRange.End;

    public bool OverlapsExceptTouching(NumericRange<TRangeNumber, TAssociated> queryNumericRange) =>
        queryNumericRange.Start < End && Start < queryNumericRange.End;

    public int CompareTo(NumericRange<TRangeNumber, TAssociated>? other)
    {
        if (other is null) return 1;
        var startComparison = Start.CompareTo(other.Start);
        return startComparison != 0
            ? startComparison
            : End.CompareTo(other.End);
    }
}
