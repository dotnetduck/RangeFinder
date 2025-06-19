using RangeFinder.Core;

namespace RangeFinder.Tests;

/// <summary>
/// Dedicated test class for async functionality of RangeFinder.
/// Tests async query methods using IAsyncEnumerable with yield return.
/// </summary>
[TestFixture]
public class RangeFinderAsyncTests
{
    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var result = new List<T>();
        await foreach (var item in source)
            result.Add(item);
        return result;
    }
    
    private static readonly List<NumericRange<double, int>> TestRanges = new()
    {
        new(1.0, 2.2, 1), new(2.0, 2.5, 2),
        new(1.0, 4.0, 3), new(4.0, 5.0, 4),
        new(5.0, 6.0, 5), new(6.0, 20.0, 6)
    };

    [TestCase(2.5, 5.1, 4)]
    [TestCase(-5, 4.0, 4)]
    [TestCase(10.0, 15.0, 1)]
    public async Task QueryAsync_RangeQuery_ReturnsCorrectCount(
        double queryStart, double queryEnd, int expectedCount)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        var results = await ToListAsync(rangeFinder.QueryRangesAsync(queryStart, queryEnd));
        
        Assert.That(results, Has.Count.EqualTo(expectedCount));
    }

    [TestCase(1.5, 2)] // Point in ranges [1.0,2.2] and [1.0,4.0]
    [TestCase(2.0, 3)] // Point in ranges [2.0,2.5], [1.0,4.0], and touches [1.0,2.2]
    [TestCase(4.0, 2)] // Point at boundaries of ranges [1.0,4.0] and [4.0,5.0]
    [TestCase(0.5, 0)] // Point before all ranges
    [TestCase(25.0, 0)] // Point after all ranges
    [TestCase(5.5, 1)] // Point in range [5.0,6.0]
    [TestCase(6.0, 2)] // Point at boundary of [5.0,6.0] and [6.0,20.0]
    public async Task QueryAsync_PointQuery_ReturnsCorrectCount(
        double point, int expectedCount)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        var results = await ToListAsync(rangeFinder.QueryRangesAsync(point));
        
        Assert.That(results, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public async Task QueryAsync_ConsistentWithSyncMethods()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);

        var testCases = new[]
        {
            new { Start = 0.0, End = 1.0 },
            new { Start = 1.0, End = 2.0 },
            new { Start = 2.0, End = 3.0 },
            new { Start = 3.0, End = 4.0 },
            new { Start = 4.0, End = 5.0 },
            new { Start = 5.0, End = 6.0 },
            new { Start = 1.5, End = 4.5 },
            new { Start = 0.0, End = 10.0 },
            new { Start = 15.0, End = 25.0 }
        };

        foreach (var testCase in testCases)
        {
            // Sync results
            var syncResults = rangeFinder.QueryRanges(testCase.Start, testCase.End)
                .OrderBy(r => r.Start)
                .ThenBy(r => r.End)
                .ToArray();

            // Async results
            var asyncResults = await ToListAsync(rangeFinder.QueryRangesAsync(testCase.Start, testCase.End));
            asyncResults = asyncResults.OrderBy(r => r.Start).ThenBy(r => r.End).ToList();

            Assert.That(asyncResults, Has.Count.EqualTo(syncResults.Length),
                $"Async and sync should return same count for range [{testCase.Start}, {testCase.End}]");

            for (int i = 0; i < syncResults.Length; i++)
            {
                Assert.That(asyncResults[i].Start, Is.EqualTo(syncResults[i].Start));
                Assert.That(asyncResults[i].End, Is.EqualTo(syncResults[i].End));
                Assert.That(asyncResults[i].Value, Is.EqualTo(syncResults[i].Value));
            }
        }
    }

    [Test]
    public async Task QueryAsync_PointQueries_ConsistentWithSyncMethods()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        var testPoints = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 10.0 };

        foreach (var point in testPoints)
        {
            // Sync results
            var syncResults = rangeFinder.QueryRanges(point)
                .OrderBy(r => r.Value)
                .ToArray();

            // Async results
            var asyncResults = await ToListAsync(rangeFinder.QueryRangesAsync(point));
            asyncResults = asyncResults.OrderBy(r => r.Value).ToList();

            Assert.That(asyncResults, Has.Count.EqualTo(syncResults.Length),
                $"Async and sync should return same count for point {point}");

            for (int i = 0; i < syncResults.Length; i++)
            {
                Assert.That(asyncResults[i].Value, Is.EqualTo(syncResults[i].Value),
                    $"Async and sync should return same ranges for point {point}");
            }
        }
    }

    [Test]
    public async Task QueryAsync_CancellationToken_ThrowsWhenCancelled()
    {
        // Create a large dataset to ensure we can cancel during enumeration
        var ranges = new List<NumericRange<double, int>>();
        for (int i = 0; i < 10000; i++)
        {
            ranges.Add(new NumericRange<double, int>(i, i + 1, i));
        }

        var rangeFinder = new RangeFinder<double, int>(ranges);
        var cts = new CancellationTokenSource();

        // Cancel immediately
        cts.Cancel();

        // Should throw OperationCanceledException
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var range in rangeFinder.QueryRangesAsync(0, 9999, cts.Token))
            {
                // This should not be reached
            }
        });
    }

    [Test]
    public async Task QueryAsync_CancellationToken_StopsEnumerationWhenCancelled()
    {
        // Create a large dataset
        var ranges = new List<NumericRange<double, int>>();
        for (int i = 0; i < 5000; i++)
        {
            ranges.Add(new NumericRange<double, int>(i, i + 10, i));
        }

        var rangeFinder = new RangeFinder<double, int>(ranges);
        var cts = new CancellationTokenSource();

        var processedCount = 0;
        var maxProcessed = 100;

        try
        {
            await foreach (var range in rangeFinder.QueryRangesAsync(0, 9999, cts.Token))
            {
                processedCount++;
                if (processedCount >= maxProcessed)
                {
                    cts.Cancel(); // Cancel after processing some items
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation occurs
        }

        Assert.That(processedCount, Is.LessThanOrEqualTo(maxProcessed + 1000), 
            "Should stop enumeration relatively quickly after cancellation");
    }

    [Test]
    public async Task QueryAsync_EarlyTermination_DoesNotEnumerateUnnecessaryItems()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        var processedCount = 0;
        
        // Query that should only match first few ranges
        await foreach (var range in rangeFinder.QueryRangesAsync(0.5, 1.5))
        {
            processedCount++;
            Assert.That(range.Start, Is.LessThanOrEqualTo(1.5));
        }
        
        // Should only find ranges that actually overlap with [0.5, 1.5]
        Assert.That(processedCount, Is.EqualTo(2)); // Should find [1.0,2.2] and [1.0,4.0]
    }

    [Test]
    public async Task QueryAsync_EmptyDataset_ReturnsEmptyAsyncEnumerable()
    {
        var emptyRanges = new List<NumericRange<double, int>>();
        var rangeFinder = new RangeFinder<double, int>(emptyRanges);
        
        var rangeResults = await ToListAsync(rangeFinder.QueryRangesAsync(1.0, 2.0));
        Assert.That(rangeResults, Is.Empty);
        
        // Point query
        var pointResults = await ToListAsync(rangeFinder.QueryRangesAsync(1.0));
        Assert.That(pointResults, Is.Empty);
    }

    [Test]
    public async Task QueryAsync_PartialEnumeration_WorksCorrectly()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRanges);
        
        var firstTwo = new List<NumericRange<double, int>>();
        
        // Take only first 2 results
        await foreach (var range in rangeFinder.QueryRangesAsync(0, 20))
        {
            firstTwo.Add(range);
            if (firstTwo.Count >= 2)
                break; // Early exit from enumeration
        }
        
        Assert.That(firstTwo, Has.Count.EqualTo(2));
        
        // Should still get valid ranges
        foreach (var range in firstTwo)
        {
            Assert.That(range.Start, Is.GreaterThanOrEqualTo(0));
            Assert.That(range.End, Is.LessThanOrEqualTo(20));
        }
    }

    [Test]
    public async Task QueryAsync_LargeDataset_StreamingPerformance()
    {
        // Create a large dataset to test streaming behavior
        var ranges = new List<NumericRange<double, int>>();
        for (int i = 0; i < 1000; i++)
        {
            ranges.Add(new NumericRange<double, int>(i, i + 5, i));
        }

        var rangeFinder = new RangeFinder<double, int>(ranges);

        // Test that we can process results one by one without loading all into memory
        var processedCount = 0;
        var maxToProcess = 50;

        await foreach (var range in rangeFinder.QueryRangesAsync(0, 999))
        {
            processedCount++;
            Assert.That(range.Value, Is.GreaterThanOrEqualTo(0));
            
            if (processedCount >= maxToProcess)
                break; // Stop early to test streaming
        }

        Assert.That(processedCount, Is.EqualTo(maxToProcess));
    }
}