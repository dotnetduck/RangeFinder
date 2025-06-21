using FsCheck;
using FsCheck.NUnit;
using IntervalTree;
using RangeFinder.Core;

namespace RangeFinder.Tests.PropertyBased;

/// <summary>
/// Property-based compatibility tests using FsCheck that verify RangeFinder behavior 
/// against IntervalTree using custom generators and factory methods.
/// </summary>
[TestFixture]
public class CompatibilityTests
{
    [OneTimeSetUp]
    public void SetUp()
    {
        Arb.Register(typeof(RangeDataGenerators));
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
                comparison.PrintRangeDebugInfo("RangeFinder vs IntervalTree mismatch", query, rangeData);
                return comparison.AreEqual;
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

                return pointResults.CompareAsSets(rangeResults).AreEqual;
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

                return results1.CompareAsSets(results2).AreEqual;
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

                return results1.CompareAsSets(results2).AreEqual;
            })
            .QuickCheckThrowOnFailure();
    }
}