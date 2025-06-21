using System;
using System.Numerics;
using RangeFinder.Tests.Helper;

namespace RangeFinder.Tests.PropertyBased;

public class Printer
{    
    /// <summary>
    /// Logs failure with context and comparison details for property-based tests
    /// Always prints detailed information when comparisons fail
    /// </summary>
    internal static bool LogAndReturn<TNumber>(
        SetDifference<int> comparison,
        string testName,
        (TNumber start, TNumber end) query,
        (TNumber start, TNumber end)[] rangeData,
        bool verbose)
        where TNumber : INumber<TNumber>
    {
        if (!comparison.AreEqual)
        {
            var debugMsg = comparison.FormatRangeDebugMessage(testName, query, rangeData);
            Console.WriteLine($"\n=== PROPERTY TEST FAILURE ===");
            Console.WriteLine(debugMsg);
            Console.WriteLine("================================================\n");
            TestContext.WriteLine($"\n=== PROPERTY TEST FAILURE) ===");
            TestContext.WriteLine(debugMsg);
            TestContext.WriteLine("================================================\n");
        }
        else if (verbose)
        {
            var successMsg = $"{testName} passed: Query=[{query.start}, {query.end}], RangeCount={rangeData.Length}";
            Console.WriteLine(successMsg);
            TestContext.WriteLine(successMsg);
        }
        return comparison.AreEqual;
    }
}
