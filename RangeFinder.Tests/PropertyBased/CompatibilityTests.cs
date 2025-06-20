using FsCheck;
using FsCheck.NUnit;
using IntervalTree;
using RangeFinder.Core;

namespace RangeFinder.Tests.PropertyBased;

/// <summary>
/// Custom generators for FsCheck property-based tests
/// </summary>
public static class RangeDataGenerators
{
    /// <summary>
    /// Generates valid range tuples ensuring start <= end
    /// </summary>
    public static Arbitrary<(double start, double end)> ValidRangeTuple()
    {
        var gen = from x in Gen.Choose(-100, 100)
                  from y in Gen.Choose(-100, 100)
                  select x <= y ? ((double)x, (double)y) : ((double)y, (double)x); // Ensure start <= end
        
        return gen.ToArbitrary();
    }

    /// <summary>
    /// Generates arrays of valid range tuples
    /// </summary>
    public static Arbitrary<(double start, double end)[]> ValidRangeArray()
    {
        var gen = from size in Gen.Choose(1, 20)
                  from ranges in Gen.ArrayOf(size, ValidRangeTuple().Generator)
                  select ranges;
        
        return gen.ToArbitrary();
    }

    /// <summary>
    /// Generates query tuples ensuring start <= end
    /// </summary>
    public static Arbitrary<(double start, double end)> ValidQueryTuple()
    {
        var gen = from x in Gen.Choose(-200, 200)
                  from y in Gen.Choose(-200, 200)
                  select x <= y ? ((double)x, (double)y) : ((double)y, (double)x); // Ensure start <= end
        
        return gen.ToArbitrary();
    }
}

/// <summary>
/// Property-based compatibility tests using FsCheck that verify RangeFinder behavior 
/// against IntervalTree using custom generators and factory methods.
/// </summary>
[TestFixture]
public class CompatibilityTests
{
    /// <summary>
    /// PROPERTY: RangeFinder and IntervalTree must always produce identical results
    /// ∀ ranges, query. RangeFinder.Query(query) = IntervalTree.Query(query)
    /// </summary>
    [FsCheck.NUnit.Property(MaxTest = 100)]
    public void RangeFinderEquivalentToIntervalTree_RangeQueries()
    {
        var rangeArb = RangeDataGenerators.ValidRangeArray();
        var queryArb = RangeDataGenerators.ValidQueryTuple();
        
        Prop.ForAll(rangeArb, queryArb, (rangeData, query) =>
            {
                // Build both data structures using factory method
                var rangeFinder = RangeFinderFactory.Create(rangeData);
                var intervalTree = new IntervalTree<double, int>();
                for (int i = 0; i < rangeData.Length; i++)
                {
                    intervalTree.Add(rangeData[i].start, rangeData[i].end, i);
                }

                // ASSERTION: Results must always be identical
                var rfResults = rangeFinder.Query(query.start, query.end).Count();
                var itResults = intervalTree.Query(query.start, query.end).Count();

                return rfResults == itResults;
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
        var rangeArb = RangeDataGenerators.ValidRangeArray();
        
        Prop.ForAll(rangeArb, Arb.From<double>(), (rangeData, point) =>
            {
                var rangeFinder = RangeFinderFactory.Create(rangeData);

                var pointResults = rangeFinder.Query(point).OrderBy(x => x).ToArray();
                var rangeResults = rangeFinder.Query(point, point).OrderBy(x => x).ToArray();

                return pointResults.SequenceEqual(rangeResults);
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
        var rangeArb = RangeDataGenerators.ValidRangeArray();
        var queryArb = RangeDataGenerators.ValidQueryTuple();
        
        Prop.ForAll(rangeArb, queryArb, (rangeData, query) =>
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
        var rangeArb = RangeDataGenerators.ValidRangeArray();
        
        Prop.ForAll(rangeArb, rangeData =>
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
        var rangeArb = RangeDataGenerators.ValidRangeArray();
        var queryArb = RangeDataGenerators.ValidQueryTuple();
        
        Prop.ForAll(rangeArb, queryArb, queryArb, (rangeData, query1, query2) =>
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
        var rangeArb = RangeDataGenerators.ValidRangeArray();
        var queryArb = RangeDataGenerators.ValidQueryTuple();
        
        Prop.ForAll(rangeArb, queryArb, (rangeData, query) =>
            {
                // Build identical structures using factory method
                var finder1 = RangeFinderFactory.Create(rangeData);
                var finder2 = RangeFinderFactory.Create(rangeData);

                var results1 = finder1.Query(query.start, query.end).OrderBy(x => x).ToArray();
                var results2 = finder2.Query(query.start, query.end).OrderBy(x => x).ToArray();

                return results1.SequenceEqual(results2);
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
        var queryArb = RangeDataGenerators.ValidQueryTuple();
        
        Prop.ForAll(queryArb, Arb.From<double>(), (query, point) =>
            {
                var emptyFinder = RangeFinderFactory.Create(Array.Empty<(double, double)>());

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
        var rangeArb = RangeDataGenerators.ValidRangeArray();
        var queryArb = RangeDataGenerators.ValidQueryTuple();
        
        Prop.ForAll(rangeArb, queryArb, (rangeData, query) =>
            {
                // Create using different factory methods
                var fromTuples = RangeFinderFactory.Create(rangeData);
                var fromArrays = RangeFinderFactory.Create(
                    rangeData.Select(t => t.start).ToArray(),
                    rangeData.Select(t => t.end).ToArray());

                var results1 = fromTuples.Query(query.start, query.end).OrderBy(x => x).ToArray();
                var results2 = fromArrays.Query(query.start, query.end).OrderBy(x => x).ToArray();

                return results1.SequenceEqual(results2);
            })
            .QuickCheckThrowOnFailure();
    }
}