using IntervalTree;
using RangeFinder.Core;

namespace RangeFinder.Tests.PropertyBased;

/// <summary>
/// Property-based compatibility tests that verify RangeFinder behavior against IntervalTree
/// using the existing test infrastructure and simple random data generation.
/// </summary>
[TestFixture]
public class CompatibilityTests
{
    private readonly Random _random = new(42); // Fixed seed for reproducibility

    /// <summary>
    /// Tests that RangeFinder and IntervalTree produce identical results for random range queries
    /// </summary>
    [Test]
    public void RangeFinderEquivalentToIntervalTree_RandomRangeQueries()
    {
        for (int iteration = 0; iteration < 100; iteration++)
        {
            // Generate random range data
            var rangeCount = _random.Next(1, 50);
            var validRanges = GenerateRandomRanges(rangeCount);

            // Generate random query
            var queryStart = _random.NextDouble() * 100;
            var queryEnd = _random.NextDouble() * 100;
            var start = Math.Min(queryStart, queryEnd);
            var end = Math.Max(queryStart, queryEnd);

            // Build both data structures
            var rangeFinder = RangeFinderFactory.Create(validRanges);
            var intervalTree = new IntervalTree<double, int>();
            for (int i = 0; i < validRanges.Length; i++)
            {
                intervalTree.Add(validRanges[i].Item1, validRanges[i].Item2, i);
            }

            // Verify results are identical
            var rfResults = rangeFinder.Query(start, end).Count();
            var itResults = intervalTree.Query(start, end).Count();

            Assert.That(rfResults, Is.EqualTo(itResults), 
                $"Results differ for iteration {iteration} with query [{start:F2}, {end:F2}] on {rangeCount} ranges");
        }
    }

    /// <summary>
    /// Tests that point queries equal range queries with same bounds
    /// </summary>
    [Test]
    public void PointQueryEqualsRangeQuery_RandomData()
    {
        for (int iteration = 0; iteration < 50; iteration++)
        {
            var rangeCount = _random.Next(1, 30);
            var validRanges = GenerateRandomRanges(rangeCount);
            var point = _random.NextDouble() * 100;

            var rangeFinder = RangeFinderFactory.Create(validRanges);

            var pointResults = rangeFinder.Query(point).OrderBy(x => x).ToArray();
            var rangeResults = rangeFinder.Query(point, point).OrderBy(x => x).ToArray();

            Assert.That(pointResults, Is.EqualTo(rangeResults),
                $"Point query differs from range query at iteration {iteration} for point {point:F2}");
        }
    }

    /// <summary>
    /// Tests that query results only contain overlapping ranges
    /// </summary>
    [Test]
    public void QueryResultsOnlyContainOverlappingRanges_RandomData()
    {
        for (int iteration = 0; iteration < 50; iteration++)
        {
            var rangeCount = _random.Next(1, 30);
            var validRanges = GenerateRandomRanges(rangeCount);

            var queryStart = _random.NextDouble() * 100;
            var queryEnd = _random.NextDouble() * 100;
            var start = Math.Min(queryStart, queryEnd);
            var end = Math.Max(queryStart, queryEnd);

            var rangeFinder = RangeFinderFactory.Create(validRanges);
            var results = rangeFinder.QueryRanges(start, end);

            foreach (var result in results)
            {
                Assert.That(result.Overlaps(start, end), Is.True,
                    $"Range [{result.Start:F2}, {result.End:F2}] should overlap with query [{start:F2}, {end:F2}] at iteration {iteration}");
            }
        }
    }

    /// <summary>
    /// Tests count property with random data
    /// </summary>
    [Test]
    public void CountPropertyEqualsInputSize_RandomData()
    {
        for (int iteration = 0; iteration < 30; iteration++)
        {
            var rangeCount = _random.Next(1, 100);
            var validRanges = GenerateRandomRanges(rangeCount);

            var rangeFinder = RangeFinderFactory.Create(validRanges);

            Assert.That(rangeFinder.Count, Is.EqualTo(validRanges.Length),
                $"Count mismatch at iteration {iteration}");
        }
    }

    /// <summary>
    /// Tests monotonicity property: expanding query bounds never reduces results
    /// </summary>
    [Test]
    public void ExpandingQueryNeverReducesResults_RandomData()
    {
        for (int iteration = 0; iteration < 30; iteration++)
        {
            var rangeCount = _random.Next(1, 30);
            var validRanges = GenerateRandomRanges(rangeCount);

            // Generate nested queries (query2 contains query1)
            var start1 = _random.NextDouble() * 50 + 25; // [25, 75]
            var end1 = start1 + _random.NextDouble() * 10; // [start1, start1+10]
            var start2 = start1 - _random.NextDouble() * 25; // [0, start1]
            var end2 = end1 + _random.NextDouble() * 25; // [end1, end1+25]

            var rangeFinder = RangeFinderFactory.Create(validRanges);

            var results1 = rangeFinder.Query(start1, end1).ToHashSet();
            var results2 = rangeFinder.Query(start2, end2).ToHashSet();

            Assert.That(results1.IsSubsetOf(results2), Is.True,
                $"Smaller query results should be subset of larger query at iteration {iteration}");
        }
    }

    /// <summary>
    /// Tests that operations are deterministic
    /// </summary>
    [Test]
    public void QueriesAreDeterministic_RandomData()
    {
        for (int iteration = 0; iteration < 20; iteration++)
        {
            var rangeCount = _random.Next(1, 30);
            var validRanges = GenerateRandomRanges(rangeCount);

            var queryStart = _random.NextDouble() * 100;
            var queryEnd = _random.NextDouble() * 100;
            var start = Math.Min(queryStart, queryEnd);
            var end = Math.Max(queryStart, queryEnd);

            // Build identical structures
            var finder1 = RangeFinderFactory.Create(validRanges);
            var finder2 = RangeFinderFactory.Create(validRanges);

            var results1 = finder1.Query(start, end).OrderBy(x => x).ToArray();
            var results2 = finder2.Query(start, end).OrderBy(x => x).ToArray();

            Assert.That(results1, Is.EqualTo(results2),
                $"Determinism failed at iteration {iteration}");
        }
    }

    /// <summary>
    /// Tests empty dataset behavior
    /// </summary>
    [Test]
    public void EmptyDatasetAlwaysProducesEmptyResults()
    {
        var emptyFinder = RangeFinderFactory.Create(Array.Empty<(double, double)>());

        for (int iteration = 0; iteration < 10; iteration++)
        {
            var queryStart = _random.NextDouble() * 100;
            var queryEnd = _random.NextDouble() * 100;
            var point = _random.NextDouble() * 100;
            var start = Math.Min(queryStart, queryEnd);
            var end = Math.Max(queryStart, queryEnd);

            var rangeResults = emptyFinder.Query(start, end).ToArray();
            var pointResults = emptyFinder.Query(point).ToArray();

            Assert.That(rangeResults, Is.Empty, $"Range query should be empty at iteration {iteration}");
            Assert.That(pointResults, Is.Empty, $"Point query should be empty at iteration {iteration}");
        }
    }

    /// <summary>
    /// Generates valid random ranges ensuring start <= end
    /// </summary>
    private (double, double)[] GenerateRandomRanges(int count)
    {
        var ranges = new (double, double)[count];
        for (int i = 0; i < count; i++)
        {
            var start = _random.NextDouble() * 100;
            var end = start + _random.NextDouble() * 20; // Ensure end >= start
            ranges[i] = (start, end);
        }
        return ranges;
    }
}