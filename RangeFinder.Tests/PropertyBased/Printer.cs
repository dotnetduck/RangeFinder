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
            TestContext.Write($"\n💣PROPERTY TEST FAILURE: ");
            TestContext.WriteLine(debugMsg);
        }
        else if (verbose)
        {
            if(rangeData.Length < comparison.ActualCount)
            {
                var warningMsg = $"⚠️ WARNING: ResultCount ({comparison.ActualCount}) exceeds RangeCount ({rangeData.Length}) for query [{query.start}, {query.end}]";
                Console.WriteLine(warningMsg);
                TestContext.WriteLine(warningMsg);
            }

            var successMsg = $"{testName} passed: Query=[{query.start}, {query.end}],"+
            $"RangeCount={rangeData.Length}, ResultCount={comparison.ActualCount}";
            TestContext.WriteLine(successMsg);
        }
        return comparison.AreEqual;
    }
}
