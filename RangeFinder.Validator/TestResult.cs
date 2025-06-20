using RangeFinder.IO.Generation;

namespace RangeFinder.Validator;

/// <summary>
/// Results from a correctness validation test between RangeFinder and IntervalTree.
/// Focus: Result compatibility validation, not performance measurement.
/// </summary>
public class TestResult
{
    public Characteristic Characteristic { get; set; }
    public int Size { get; set; }
    public int QueryCount { get; set; }
    
    // Correctness validation
    public List<CompatibilityError> CompatibilityErrors { get; set; } = new();
    
    public bool IsCompatible => !CompatibilityErrors.Any();
    
    public void PrintSummary()
    {
        Console.WriteLine($"🔍 Validation Results: {Characteristic} ({Size:N0} ranges, {QueryCount:N0} queries)");
        Console.WriteLine($"   ✅ Compatibility: {(IsCompatible ? "PASS" : $"FAIL ({CompatibilityErrors.Count} errors)")}");
        
        if (!IsCompatible)
        {
            Console.WriteLine($"   ❌ First 3 errors:");
            foreach (var error in CompatibilityErrors.Take(3))
            {
                Console.WriteLine($"      {error.QueryType} {error.Query}: RF[{string.Join(",", error.RangeFinderResult)}] vs IT[{string.Join(",", error.IntervalTreeResult)}]");
            }
        }
    }

    public void PrintDetailedErrors()
    {
        if (!CompatibilityErrors.Any())
        {
            Console.WriteLine("✅ No compatibility errors found.");
            return;
        }

        Console.WriteLine($"\n❌ Found {CompatibilityErrors.Count} compatibility errors:");
        
        foreach (var error in CompatibilityErrors)
        {
            Console.WriteLine($"\n   {error.QueryType} {error.Query}:");
            Console.WriteLine($"     RangeFinder: [{string.Join(", ", error.RangeFinderResult)}]");
            Console.WriteLine($"     IntervalTree: [{string.Join(", ", error.IntervalTreeResult)}]");
            
            if (error.OnlyInRangeFinder.Any()) 
                Console.WriteLine($"     Only in RangeFinder: [{string.Join(", ", error.OnlyInRangeFinder)}]");
            if (error.OnlyInIntervalTree.Any()) 
                Console.WriteLine($"     Only in IntervalTree: [{string.Join(", ", error.OnlyInIntervalTree)}]");
        }
    }
}