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
    /// Deterministic seed for property-based tests. Change this value to reproduce specific failures.
    /// All property tests will use this seed for consistent, reproducible behavior.
    /// </summary>
    private const int TestSeed = 12345;
    
    /// <summary>
    /// Custom Property attribute that uses our const seed for deterministic testing
    /// </summary>
    public class SeededPropertyAttribute : FsCheck.NUnit.PropertyAttribute
    {
        public SeededPropertyAttribute(int maxTest = 100) : base()
        {
            MaxTest = maxTest;
            Replay = TestSeed + "," + TestSeed; // Use const value
        }
    }

    [OneTimeSetUp]
    public void SetUp()
    {
        // Display deterministic seed for property testing
        Console.WriteLine($"\n=== PROPERTY TEST SEED: {TestSeed} ===");
        Console.WriteLine("All property tests use this fixed seed for deterministic behavior");
        Console.WriteLine($"To reproduce failures, set TestSeed = {TestSeed} in CompatibilityTests.cs");
        Console.WriteLine("========================================\n");
        TestContext.WriteLine($"Property Test Seed: {TestSeed}");
        
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
    [SeededProperty(TestSeed)]
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
                return Printer.LogAndReturn(
                    comparison, "RangeFinder vs IntervalTree equivalence", query, rangeData, TestSeed, VerboseMode);
            })
            .QuickCheck();
    }

    /// <summary>
    /// PROPERTY: Point query must equal range query with same start/end
    /// ∀ ranges, point. Query(point) = Query(point, point)
    /// </summary>
    [SeededProperty(TestSeed)]
    public void PointQueryEqualsRangeQuery()
    {
        Prop.ForAll<(double start, double end)[], double>((rangeData, point) =>
            {
                var rangeFinder = RangeFinderFactory.Create(rangeData);

                var pointResults = rangeFinder.Query(point);
                var rangeResults = rangeFinder.Query(point, point);

                var comparison = pointResults.CompareAsSets(rangeResults);
                return Printer.LogAndReturn(
                    comparison, "Point vs Range query equivalence",
                    (point, point), new[] { (point, point) }, TestSeed, VerboseMode);
            })
            .QuickCheck();
    }

    /// <summary>
    /// PROPERTY: Query results must only contain ranges that actually overlap
    /// ∀ ranges, query. ∀ result ∈ Query(query). result overlaps query
    /// </summary>
    [SeededProperty(TestSeed)]
    public void QueryResultsOnlyContainOverlappingRanges()
    {
        Prop.ForAll<(double start, double end)[], (double start, double end)>((rangeData, query) =>
            {
                var rangeFinder = RangeFinderFactory.Create(rangeData);
                var results = rangeFinder.QueryRanges(query.start, query.end);

                // ASSERTION: Every result must overlap with query
                return results.All(result => result.Overlaps(query.start, query.end));
            })
            .QuickCheck();
    }

    /// <summary>
    /// PROPERTY: Count must equal input size
    /// ∀ ranges. RangeFinder(ranges).Count = |ranges|
    /// </summary>
    [SeededProperty(TestSeed)]
    public void CountPropertyEqualsInputSize()
    {
        Prop.ForAll<(double start, double end)[]>(rangeData =>
            {
                var rangeFinder = RangeFinderFactory.Create(rangeData);
                return rangeFinder.Count == rangeData.Length;
            })
            .QuickCheck();
    }

    /// <summary>
    /// PROPERTY: Expanding query bounds never reduces results (Monotonicity)
    /// ∀ ranges, q1, q2. q1 ⊆ q2 ⟹ Query(q1) ⊆ Query(q2)
    /// </summary>
    [SeededProperty(TestSeed)]
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
            .QuickCheck();
    }

    /// <summary>
    /// PROPERTY: Operations are deterministic
    /// ∀ ranges, query. Query(ranges, query) = Query(ranges, query)
    /// </summary>
    [SeededProperty(TestSeed, MaxTest = 10)]
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
                return Printer.LogAndReturn(
                    comparison, "Query determinism", query, rangeData, TestSeed, VerboseMode);
            })
            .QuickCheck();
    }

    /// <summary>
    /// PROPERTY: Empty dataset always produces empty results
    /// ∀ query. Query_on_EmptyDataset(query) = ∅
    /// </summary>
    [SeededProperty(TestSeed, MaxTest = 10)]
    public void EmptyDatasetAlwaysProducesEmptyResults()
    {
        Prop.ForAll<(double start, double end), double>((query, point) =>
            {
                var emptyFinder = RangeFinderFactory.Create(Array.Empty<(double start, double end)>());

                var rangeResults = emptyFinder.Query(query.start, query.end).ToArray();
                var pointResults = emptyFinder.Query(point).ToArray();

                return rangeResults.Length == 0 && pointResults.Length == 0;
            })
            .QuickCheck();
    }

    /// <summary>
    /// PROPERTY: Factory methods produce equivalent results
    /// ∀ ranges, query. FromTuples(ranges).Query(query) = FromArrays(ranges).Query(query)
    /// </summary>
    [SeededProperty(TestSeed, MaxTest = 10)]
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
                return Printer.LogAndReturn(
                    comparison, "Factory method equivalence", query, rangeData, TestSeed, VerboseMode);
            })
            .QuickCheck();
    }

}