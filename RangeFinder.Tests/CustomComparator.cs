using System.Numerics;
using System.Text;
using RangeFinder.Core;
using RangeFinder.IO.Serialization;

namespace RangeFinder.Tests;

/// <summary>
/// Custom assertions for property-based testing
/// </summary>
public static class CustomComparator
{
    /// <summary>
    /// Compares this sequence with another as sets and returns detailed difference information
    /// </summary>
    public static SetDifference<T> CompareAsSets<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
    {
        var expectedSet = expected.ToHashSet();
        var actualSet = actual.ToHashSet();
        
        var onlyInExpected = expectedSet.Except(actualSet).ToHashSet();
        var onlyInActual = actualSet.Except(expectedSet).ToHashSet();
        
        return new SetDifference<T>(onlyInExpected, onlyInActual);
    }
}

/// <summary>
/// Contains difference information between two sets
/// </summary>
public record SetDifference<T>(HashSet<T> OnlyInExpected, HashSet<T> OnlyInActual)
{
    /// <summary>
    /// True if both sets are equal (no differences)
    /// </summary>
    public bool AreEqual => OnlyInExpected.Count == 0 && OnlyInActual.Count == 0;
    
    /// <summary>
    /// Creates a human-readable description of the differences
    /// </summary>
    public string GetDescription()
    {
        if (AreEqual) return "Sets are equal";
        
        var parts = new List<string>();
        if (OnlyInExpected.Count > 0)
            parts.Add($"Only in expected: [{string.Join(", ", OnlyInExpected)}]");
        if (OnlyInActual.Count > 0)
            parts.Add($"Only in actual: [{string.Join(", ", OnlyInActual)}]");
            
        return string.Join("; ", parts);
    }
    
    /// <summary>
    /// Prints detailed debug information if sets are not equal
    /// </summary>
    public void PrintDebugInfo(string context = "Set comparison")
    {
        if (!AreEqual)
        {
            Console.WriteLine($"{context} failed:");
            Console.WriteLine($"  {GetDescription()}");
        }
    }
    
    /// <summary>
    /// Prints detailed debug information with additional context data
    /// </summary>
    public void PrintDebugInfo<TContext>(string context, TContext contextData)
    {
        if (!AreEqual)
        {
            Console.WriteLine($"{context} failed:");
            Console.WriteLine($"  Context: {contextData}");
            Console.WriteLine($"  {GetDescription()}");
        }
    }
    
    /// <summary>
    /// Prints detailed debug information for range comparison failures with data export
    /// </summary>
    public void PrintRangeDebugInfo<TNumber>(string context, (TNumber start, TNumber end) query, (TNumber start, TNumber end)[] rangeData)
        where TNumber : INumber<TNumber>
    {
        if (!AreEqual)
        {
            var debugMsg = FormatRangeDebugMessage(context, query, rangeData);
            Console.WriteLine(debugMsg);
            TestContext.WriteLine(debugMsg);
        }
    }
    
    /// <summary>
    /// Formats a comprehensive debug message for range comparison failures
    /// </summary>
    public string FormatRangeDebugMessage<TNumber>(string context, (TNumber start, TNumber end) query, (TNumber start, TNumber end)[] rangeData)
        where TNumber : INumber<TNumber>
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{context} failed:");
        sb.AppendLine($"  Query: [{query.start}, {query.end}]");
        
        if (rangeData.Length <= 10)
        {
            sb.AppendLine($"  RangeData: [{string.Join(", ", rangeData.Select(r => $"({r.start},{r.end})"))}]");
        }
        else
        {
            // Export to CSV using existing RangeSerializer
            var fileName = $"debug_ranges_{DateTime.Now:yyyyMMdd_HHmmss_fff}.csv";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            try
            {
                // Convert tuples to NumericRange for serialization
                var ranges = rangeData.Select((r, i) => new NumericRange<TNumber, int>(r.start, r.end, i));
                ranges.WriteCsv(filePath);
                
                sb.AppendLine($"  RangeData ({rangeData.Length} ranges): [{string.Join(", ", rangeData.Take(5).Select(r => $"({r.start},{r.end})"))}] ... (full data saved to {filePath})");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  RangeData ({rangeData.Length} ranges): [{string.Join(", ", rangeData.Take(5).Select(r => $"({r.start},{r.end})"))}] ... (failed to save CSV: {ex.Message})");
            }
        }
        
        sb.AppendLine($"  {GetDescription()}");
        return sb.ToString();
    }
    
    /// <summary>
    /// Logs failure with context and comparison details for property-based tests
    /// </summary>
    public bool LogAndReturn<TNumber>(string testName, (TNumber start, TNumber end) query, (TNumber start, TNumber end)[] rangeData)
        where TNumber : INumber<TNumber>
    {
        if (!AreEqual)
        {
            PrintRangeDebugInfo(testName, query, rangeData);
        }
        return AreEqual;
    }
}
