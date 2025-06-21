using FsCheck;
using FsCheck.NUnit;
using IntervalTree;
using RangeFinder.Core;
using System.Numerics;
using RangeFinder.Tests.Helper;

namespace RangeFinder.Tests.PropertyBased;

/// <summary>
/// Property-based compatibility tests using FsCheck that verify RangeFinder behavior 
/// against IntervalTree using custom generators and factory methods.
/// </summary>
[TestFixture]
public class CompatibilityTests
{
    /// <summary>
    /// Enable verbose mode to print results for all property tests (success and failure)
    /// Set to true for detailed debugging, false for normal operation
    /// </summary>
    private static readonly bool VerboseMode = false;
    
    /// <summary>
    /// Logs failure with context and comparison details for property-based tests
    /// </summary>
    private static bool LogAndReturn<TNumber>(SetDifference<int> comparison, string testName, (TNumber start, TNumber end) query, (TNumber start, TNumber end)[] rangeData)
        where TNumber : INumber<TNumber>
    {
        if (!comparison.AreEqual)
        {
            var debugMsg = comparison.FormatRangeDebugMessage(testName, query, rangeData);
            Console.WriteLine(debugMsg);
            TestContext.WriteLine(debugMsg);
        }
        else if (VerboseMode)
        {
            var successMsg = $"{testName} passed: Query=[{query.start}, {query.end}], RangeCount={rangeData.Length}";
            Console.WriteLine(successMsg);
            TestContext.WriteLine(successMsg);
        }
        return comparison.AreEqual;
    }
    
    [OneTimeSetUp]
    public void SetUp()
    {
        Arb.Register(typeof(RangeDataGenerators));
        
        if (VerboseMode)
        {
            Console.WriteLine("Verbose mode enabled: will print results for all property tests");
            TestContext.WriteLine("Verbose mode enabled: will print results for all property tests");
        }
    }

    /// <summary>
    /// PROPERTY: RangeFinder and IntervalTree must always produce identical results
    /// ∀ ranges, query. RangeFinder.Query(query) = IntervalTree.Query(query)
    /// </summary>
    [FsCheck.NUnit.Property]
    public void RangeFinderEquivalentToIntervalTree_RangeQueries()
    {
        Prop.ForAll<(double start, double end)[], (double start, double end)>((rangeData, query) =>
            {
                // Build both data structures using factory method
                var rangeFinder = RangeFinderFactory.Create(rangeData);
                var intervalTree = new IntervalTree<double, int>();
                for (int i = 0; i < rangeData.Length; i++)
                {
                    intervalTree.Add(rangeData[i].start, rangeData[i].end, i);
                }

                // ASSERTION: Results must be identical (same elements, order doesn't matter)
                var rfResults = rangeFinder.Query(query.start, query.end);
                var itResults = intervalTree.Query(query.start, query.end);

                var comparison = rfResults.CompareAsSets(itResults);
                return LogAndReturn(comparison, "RangeFinder vs IntervalTree equivalence", query, rangeData);
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// PROPERTY: Point query must equal range query with same start/end
    /// ∀ ranges, point. Query(point) = Query(point, point)
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 50)]
    public void PointQueryEqualsRangeQuery()
    {
        Prop.ForAll<(double start, double end)[], double>((rangeData, point) =>
            {
                var rangeFinder = RangeFinderFactory.Create(rangeData);

                var pointResults = rangeFinder.Query(point);
                var rangeResults = rangeFinder.Query(point, point);

                var comparison = pointResults.CompareAsSets(rangeResults);
                return LogAndReturn(comparison, "Point vs Range query equivalence", (point, point), new[] { (point, point) });
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// PROPERTY: Query results must only contain ranges that actually overlap
    /// ∀ ranges, query. ∀ result ∈ Query(query). result overlaps query
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 50)]
    public void QueryResultsOnlyContainOverlappingRanges()
    {
        Prop.ForAll<(double start, double end)[], (double start, double end)>((rangeData, query) =>
            {
                var rangeFinder = RangeFinderFactory.Create(rangeData);
                var results = rangeFinder.QueryRanges(query.start, query.end);

                // ASSERTION: Every result must overlap with query
                return results.All(result => result.Overlaps(query.start, query.end));
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// PROPERTY: Count must equal input size
    /// ∀ ranges. RangeFinder(ranges).Count = |ranges|
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 50)]
    public void CountPropertyEqualsInputSize()
    {
        Prop.ForAll<(double start, double end)[]>(rangeData =>
            {
                var rangeFinder = RangeFinderFactory.Create(rangeData);
                return rangeFinder.Count == rangeData.Length;
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// PROPERTY: Expanding query bounds never reduces results (Monotonicity)
    /// ∀ ranges, q1, q2. q1 ⊆ q2 ⟹ Query(q1) ⊆ Query(q2)
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 30)]
    public void ExpandingQueryNeverReducesResults()
    {
        Prop.ForAll<(double start, double end)[], (double start, double end), (double start, double end)>((rangeData, query1, query2) =>
            {
                // Ensure query2 contains query1
                var expandedQuery = (
                    Math.Min(query1.start, query2.start),
                    Math.Max(query1.end, query2.end)
                );

                var rangeFinder = RangeFinderFactory.Create(rangeData);

                // ASSERTION: Larger query must contain all results from smaller query
                var results1 = rangeFinder.Query(query1.start, query1.end).ToHashSet();
                var results2 = rangeFinder.Query(expandedQuery.Item1, expandedQuery.Item2).ToHashSet();

                return results1.IsSubsetOf(results2);
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// PROPERTY: Operations are deterministic
    /// ∀ ranges, query. Query(ranges, query) = Query(ranges, query)
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 30)]
    public void QueriesAreDeterministic()
    {
        Prop.ForAll<(double start, double end)[], (double start, double end)>((rangeData, query) =>
            {
                // Build identical structures using factory method
                var finder1 = RangeFinderFactory.Create(rangeData);
                var finder2 = RangeFinderFactory.Create(rangeData);

                var results1 = finder1.Query(query.start, query.end);
                var results2 = finder2.Query(query.start, query.end);

                var comparison = results1.CompareAsSets(results2);
                return LogAndReturn(comparison, "Query determinism", query, rangeData);
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// PROPERTY: Empty dataset always produces empty results
    /// ∀ query. Query_on_EmptyDataset(query) = ∅
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 20)]
    public void EmptyDatasetAlwaysProducesEmptyResults()
    {
        Prop.ForAll<(double start, double end), double>((query, point) =>
            {
                var emptyFinder = RangeFinderFactory.Create(Array.Empty<(double start, double end)>());

                var rangeResults = emptyFinder.Query(query.start, query.end).ToArray();
                var pointResults = emptyFinder.Query(point).ToArray();

                return rangeResults.Length == 0 && pointResults.Length == 0;
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// PROPERTY: Factory methods produce equivalent results
    /// ∀ ranges, query. FromTuples(ranges).Query(query) = FromArrays(ranges).Query(query)
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 20)]
    public void FactoryMethodsProduceEquivalentResults()
    {
        Prop.ForAll<(double start, double end)[], (double start, double end)>((rangeData, query) =>
            {
                // Create using different factory methods
                var fromTuples = RangeFinderFactory.Create(rangeData);
                var fromArrays = RangeFinderFactory.Create(
                    rangeData.Select(t => t.start).ToArray(),
                    rangeData.Select(t => t.end).ToArray());

                var results1 = fromTuples.Query(query.start, query.end);
                var results2 = fromArrays.Query(query.start, query.end);

                var comparison = results1.CompareAsSets(results2);
                return LogAndReturn(comparison, "Factory method equivalence", query, rangeData);
            })
            .QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// FAULT INJECTION TEST: Intentionally broken test to verify detection capabilities
    /// This test should ALWAYS FAIL to verify our test infrastructure works correctly.
    /// Uses large datasets (15+ ranges) to test CSV serialization functionality.
    /// Normally commented out - only enable when testing the test framework itself.
    /// </summary>
    // [FsCheck.NUnit.Property(MaxTest = 5)]
    public void FaultInjection_IntentionallyBrokenTest()
    {
        Prop.ForAll<(double start, double end)[], (double start, double end)>((rangeData, query) =>
            {
                // Force large dataset - pad with extra ranges if needed
                var paddedRangeData = rangeData.Length >= 15 ? rangeData 
                    : rangeData.Concat(Enumerable.Range(0, 15 - rangeData.Length)
                                     .Select(i => (start: (double)i, end: (double)i + 10)))
                                     .ToArray();
                // Build RangeFinder normally
                var rangeFinder = RangeFinderFactory.Create(paddedRangeData);
                
                // Build IntervalTree with INTENTIONALLY WRONG logic (inject fault)
                var intervalTree = new IntervalTree<double, int>();
                for (int i = 0; i < paddedRangeData.Length; i++)
                {
                    // FAULT: Add ranges with offset to cause mismatch (avoid ArgumentOutOfRangeException)
                    var faultyStart = paddedRangeData[i].start + 1000;  // Intentionally offset!
                    var faultyEnd = paddedRangeData[i].end + 1000;      // Intentionally offset!
                    intervalTree.Add(faultyStart, faultyEnd, i);
                }

                // ASSERTION: This should detect the intentional fault
                var rfResults = rangeFinder.Query(query.start, query.end);
                var itResults = intervalTree.Query(query.start, query.end);

                var comparison = rfResults.CompareAsSets(itResults);
                
                // This should return false most of the time due to injected fault
                return LogAndReturn(comparison, "FAULT INJECTION: Expected mismatch", query, paddedRangeData);
            })
            .QuickCheckThrowOnFailure();
    }
}