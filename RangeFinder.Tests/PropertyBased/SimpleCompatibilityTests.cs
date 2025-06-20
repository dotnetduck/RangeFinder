using IntervalTree;
using RangeFinder.Core;
using RangeFinder.IO.Generation;
using RangeFinder.Tests;

namespace RangeFinder.Tests.PropertyBased;

/// <summary>
/// Simplified property-based tests validating RangeFinder vs IntervalTree compatibility.
/// Uses the existing Generator utilities for realistic test data without complex FsCheck setup.
/// </summary>
[TestFixture]
public class SimpleCompatibilityTests : TestBase
{
    /// <summary>
    /// Validates that RangeFinder and IntervalTree produce identical results
    /// across all characteristics with the efficient "1 dataset : many queries" approach.
    /// </summary>
    [Test]
    public void RangeFinderAndIntervalTreeProduceIdenticalResults()
    {
        var characteristics = new[]
        {
            Characteristic.Uniform,
            Characteristic.DenseOverlapping,
            Characteristic.SparseNonOverlapping,
            Characteristic.Clustered
        };

        var testSizes = new[] { 100, 500, 1000 };
        var queryCount = 50;

        foreach (var characteristic in characteristics)
        {
            foreach (var size in testSizes)
            {
                // Generate ONE dataset using existing proven generators
                var parameters = GetParameters(characteristic, size);
                var ranges = Generator.GenerateRanges<double>(parameters);
                var queryRanges = Generator.GenerateQueryRanges<double>(parameters, queryCount);
                var queryPoints = Generator.GenerateQueryPoints<double>(parameters, queryCount);

                // Build both structures ONCE - amortize construction cost
                var rangeFinder = new RangeFinder<double, int>(ranges);
                var intervalTree = new IntervalTree<double, int>();
                ranges.ForEach(r => intervalTree.Add(r.Start, r.End, r.Value));

                // Test MANY range queries against the same dataset
                foreach (var query in queryRanges)
                {
                    var rfResults = rangeFinder.Query(query.Start, query.End)
                        .OrderBy(x => x).ToArray();
                    var itResults = intervalTree.Query(query.Start, query.End)
                        .OrderBy(x => x).ToArray();

                    Assert.That(rfResults.SequenceEqual(itResults), Is.True,
                        $"Range query [{query.Start:F3}, {query.End:F3}] failed for {characteristic} with {size} ranges");
                }

                // Test MANY point queries against the same dataset
                foreach (var point in queryPoints)
                {
                    var rfResults = rangeFinder.Query(point)
                        .OrderBy(x => x).ToArray();
                    var itResults = intervalTree.Query(point)
                        .OrderBy(x => x).ToArray();

                    Assert.That(rfResults.SequenceEqual(itResults), Is.True,
                        $"Point query {point:F3} failed for {characteristic} with {size} ranges");
                }
            }
        }
    }

    /// <summary>
    /// Validates that point queries and equivalent range queries produce identical results.
    /// </summary>
    [Test]
    public void PointQueryEqualsRangeQuery()
    {
        var characteristics = new[]
        {
            Characteristic.Uniform,
            Characteristic.DenseOverlapping,
            Characteristic.SparseNonOverlapping,
            Characteristic.Clustered
        };

        foreach (var characteristic in characteristics)
        {
            var parameters = GetParameters(characteristic, 500);
            var ranges = Generator.GenerateRanges<double>(parameters);
            var queryPoints = Generator.GenerateQueryPoints<double>(parameters, 30);

            var rangeFinder = new RangeFinder<double, int>(ranges);

            foreach (var point in queryPoints)
            {
                var pointResults = rangeFinder.Query(point)
                    .OrderBy(x => x).ToArray();
                var rangeResults = rangeFinder.Query(point, point)
                    .OrderBy(x => x).ToArray();

                Assert.That(pointResults.SequenceEqual(rangeResults), Is.True,
                    $"Point query vs range query mismatch at {point:F3} for {characteristic}");
            }
        }
    }

    /// <summary>
    /// Validates that query results contain only ranges that actually overlap with the query.
    /// </summary>
    [Test]
    public void QueryResultsContainOnlyOverlappingRanges()
    {
        var characteristics = new[]
        {
            Characteristic.Uniform,
            Characteristic.DenseOverlapping,
            Characteristic.SparseNonOverlapping,
            Characteristic.Clustered
        };

        foreach (var characteristic in characteristics)
        {
            var parameters = GetParameters(characteristic, 1000);
            var ranges = Generator.GenerateRanges<double>(parameters);
            var queryRanges = Generator.GenerateQueryRanges<double>(parameters, 50);

            var rangeFinder = new RangeFinder<double, int>(ranges);

            foreach (var query in queryRanges)
            {
                var results = rangeFinder.QueryRanges(query.Start, query.End);

                foreach (var result in results)
                {
                    Assert.That(result.Overlaps(query.Start, query.End), Is.True,
                        $"Result range [{result.Start:F3}, {result.End:F3}] does not overlap with query [{query.Start:F3}, {query.End:F3}] for {characteristic}");
                }
            }
        }
    }

    /// <summary>
    /// Validates that expanding query bounds never reduces results.
    /// </summary>
    [Test]
    public void ExpandingQueryBoundsNeverReducesResults()
    {
        var random = new Random(42);
        var characteristics = new[]
        {
            Characteristic.Uniform,
            Characteristic.DenseOverlapping,
            Characteristic.SparseNonOverlapping,
            Characteristic.Clustered
        };

        foreach (var characteristic in characteristics)
        {
            var parameters = GetParameters(characteristic, 500);
            var ranges = Generator.GenerateRanges<double>(parameters);
            var rangeFinder = new RangeFinder<double, int>(ranges);

            // Test multiple query expansions
            for (int i = 0; i < 20; i++)
            {
                // Generate a smaller query
                var start1 = random.NextDouble() * parameters.TotalSpace * 0.8;
                var end1 = start1 + random.NextDouble() * parameters.TotalSpace * 0.1;

                // Generate a larger query that contains the smaller one
                var start2 = start1 - random.NextDouble() * parameters.TotalSpace * 0.1;
                var end2 = end1 + random.NextDouble() * parameters.TotalSpace * 0.1;

                var results1 = rangeFinder.Query(start1, end1).ToHashSet();
                var results2 = rangeFinder.Query(start2, end2).ToHashSet();

                Assert.That(results1.IsSubsetOf(results2), Is.True,
                    $"Expanding query bounds reduced results for {characteristic}: " +
                    $"[{start1:F3}, {end1:F3}] -> [{start2:F3}, {end2:F3}]");
            }
        }
    }

    /// <summary>
    /// Validates that empty datasets produce empty results for any query.
    /// </summary>
    [Test]
    public void EmptyDatasetProducesEmptyResults()
    {
        var emptyRanges = new List<NumericRange<double, int>>();
        var rangeFinder = new RangeFinder<double, int>(emptyRanges);
        var intervalTree = new IntervalTree<double, int>();

        var random = new Random(42);
        for (int i = 0; i < 10; i++)
        {
            var queryStart = random.NextDouble() * 1000;
            var queryEnd = queryStart + random.NextDouble() * 100;
            var queryPoint = random.NextDouble() * 1000;

            var rfRangeResults = rangeFinder.Query(queryStart, queryEnd).ToArray();
            var itRangeResults = intervalTree.Query(queryStart, queryEnd).ToArray();
            var rfPointResults = rangeFinder.Query(queryPoint).ToArray();
            var itPointResults = intervalTree.Query(queryPoint).ToArray();

            Assert.That(rfRangeResults.Length, Is.EqualTo(0));
            Assert.That(itRangeResults.Length, Is.EqualTo(0));
            Assert.That(rfPointResults.Length, Is.EqualTo(0));
            Assert.That(itPointResults.Length, Is.EqualTo(0));
        }
    }

    /// <summary>
    /// Stress test with larger datasets to ensure compatibility at scale.
    /// </summary>
    [Test]
    public void LargeDatasetCompatibility()
    {
        var characteristics = new[]
        {
            Characteristic.Uniform,
            Characteristic.DenseOverlapping,
            Characteristic.SparseNonOverlapping,
            Characteristic.Clustered
        };

        foreach (var characteristic in characteristics)
        {
            var parameters = GetParameters(characteristic, 5000);
            var ranges = Generator.GenerateRanges<double>(parameters);
            var queryRanges = Generator.GenerateQueryRanges<double>(parameters, 100);

            var rangeFinder = new RangeFinder<double, int>(ranges);
            var intervalTree = new IntervalTree<double, int>();
            ranges.ForEach(r => intervalTree.Add(r.Start, r.End, r.Value));

            foreach (var query in queryRanges)
            {
                var rfResults = rangeFinder.Query(query.Start, query.End)
                    .OrderBy(x => x).ToArray();
                var itResults = intervalTree.Query(query.Start, query.End)
                    .OrderBy(x => x).ToArray();

                Assert.That(rfResults.SequenceEqual(itResults), Is.True,
                    $"Large dataset compatibility failed for {characteristic} at query [{query.Start:F3}, {query.End:F3}]");
            }
        }
    }

    private static Parameter GetParameters(Characteristic characteristic, int size) => characteristic switch
    {
        Characteristic.Uniform => RangeParameterFactory.Uniform(size),
        Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(size),
        Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(size),
        Characteristic.Clustered => RangeParameterFactory.Clustered(size),
        _ => throw new ArgumentException($"Unknown characteristic: {characteristic}")
    };
}