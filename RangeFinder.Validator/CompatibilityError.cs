namespace RangeFinder.Validator;

/// <summary>
/// Represents a compatibility error between RangeFinder and IntervalTree results.
/// </summary>
public class CompatibilityError
{
    public string QueryType { get; set; } = "";
    public string Query { get; set; } = "";
    public int[] RangeFinderResult { get; set; } = Array.Empty<int>();
    public int[] IntervalTreeResult { get; set; } = Array.Empty<int>();
    public int[] OnlyInRangeFinder { get; set; } = Array.Empty<int>();
    public int[] OnlyInIntervalTree { get; set; } = Array.Empty<int>();
}