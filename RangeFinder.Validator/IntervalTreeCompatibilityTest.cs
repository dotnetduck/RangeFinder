using System.Diagnostics;
using RangeFinder.Core;
using RangeFinder.Generator;
using RangeFinder.RangeTreeCompat;
using Gen = RangeFinder.Generator.Generator;

namespace RangeFinder.Validator;

/// <summary>
/// Tests the RangeTree compatibility wrapper to ensure it provides identical results
/// to the underlying RangeFinder implementation and properly handles dynamic operations.
/// </summary>
public class IntervalTreeCompatibilityTest
{
    private readonly Random _random = new();
    private readonly Characteristic[] _characteristics = 
    {
        Characteristic.Uniform, 
        Characteristic.DenseOverlapping, 
        Characteristic.SparseNonOverlapping,
        Characteristic.Clustered
    };

    public int TotalTests { get; private set; }
    public int FailureCount { get; private set; }
    
    /// <summary>
    /// Tests basic CRUD operations and dynamic rebuilding of the IntervalTree wrapper.
    /// </summary>
    public TestResult RunDynamicOperationsTest(Characteristic characteristic, int size, int operationCount = 100)
    {
        var result = new TestResult
        {
            Characteristic = characteristic,
            Size = size,
            QueryCount = operationCount
        };

        var parameters = GetParameters(characteristic, size);
        var initialRanges = Gen.GenerateRanges<double>(parameters);
        var queries = Gen.GenerateQueryRanges<double>(parameters, operationCount / 2);
        var points = Gen.GenerateQueryPoints<double>(parameters, operationCount / 2);

        // Create IntervalTree and populate with initial data
        var intervalTree = new RangeTreeAdapter<double, int>();
        PopulateIntervalTree(intervalTree, initialRanges);

        // Create reference RangeFinder for comparison
        var rangeFinder = new RangeFinder<double, int>(initialRanges);

        var errors = new List<CompatibilityError>();

        // Test 1: Initial state compatibility
        TotalTests += 2;
        var (intervalTreeTime1, rangeFinderTime1) = ValidateQueriesHelperWithTiming(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "Initial");

        // Test 2: Add operations with dynamic rebuilding
        var newParameters = GetParameters(characteristic, size / 10);
        var newRanges = Gen.GenerateRanges<double>(newParameters);
        
        // Track current ranges for RangeFinder to perform identical operations
        var currentRanges = initialRanges.ToList();
        
        // Measure IntervalTree add operations
        var sw = Stopwatch.StartNew();
        AddRangesToIntervalTree(intervalTree, newRanges);
        var intervalTreeAddTime = sw.Elapsed.TotalMilliseconds;

        // Measure RangeFinder performing identical add operations (rebuild with same final state)
        sw.Restart();
        currentRanges.AddRange(newRanges);
        rangeFinder = new RangeFinder<double, int>(currentRanges);
        var rangeFinderRebuildTime = sw.Elapsed.TotalMilliseconds;

        TotalTests += 2;
        var (intervalTreeQueryTime2, rangeFinderQueryTime2) = ValidateQueriesHelperWithTiming(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterAdd");
        
        // Include operation costs in total time
        var intervalTreeTime2 = intervalTreeAddTime + intervalTreeQueryTime2;
        var rangeFinderTime2 = rangeFinderRebuildTime + rangeFinderQueryTime2;

        // Test 3: Remove operations
        var toRemove = currentRanges.Take(size / 20).Select(r => r.Value).ToList();
        
        // Measure IntervalTree remove operations
        sw.Restart();
        RemoveValuesFromIntervalTree(intervalTree, toRemove);
        var intervalTreeRemoveTime = sw.Elapsed.TotalMilliseconds;
        
        // Measure RangeFinder performing identical remove operations (rebuild with same final state)
        sw.Restart();
        currentRanges.RemoveAll(r => toRemove.Contains(r.Value));
        rangeFinder = new RangeFinder<double, int>(currentRanges);
        var rangeFinderRebuildTime3 = sw.Elapsed.TotalMilliseconds;

        TotalTests += 2;
        var (intervalTreeQueryTime3, rangeFinderQueryTime3) = ValidateQueriesHelperWithTiming(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterRemove");
        
        // Include operation costs in total time
        var intervalTreeTime3 = intervalTreeRemoveTime + intervalTreeQueryTime3;
        var rangeFinderTime3 = rangeFinderRebuildTime3 + rangeFinderQueryTime3;

        // Test 4: Bulk remove operations
        double intervalTreeTime4 = 0, rangeFinderTime4 = 0;
        if (currentRanges.Count > 5)
        {
            var bulkRemove = currentRanges.Take(3).Select(r => r.Value).ToList();
            
            // Measure IntervalTree bulk remove operations
            sw.Restart();
            intervalTree.Remove(bulkRemove);
            var intervalTreeBulkRemoveTime = sw.Elapsed.TotalMilliseconds;
            
            // Measure RangeFinder performing identical bulk remove operations (rebuild with same final state)
            sw.Restart();
            currentRanges.RemoveAll(r => bulkRemove.Contains(r.Value));
            rangeFinder = new RangeFinder<double, int>(currentRanges);
            var rangeFinderRebuildTime4 = sw.Elapsed.TotalMilliseconds;
            
            TotalTests += 2;
            var (intervalTreeQueryTime4, rangeFinderQueryTime4) = ValidateQueriesHelperWithTiming(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterBulkRemove");
            
            // Include operation costs in total time
            intervalTreeTime4 = intervalTreeBulkRemoveTime + intervalTreeQueryTime4;
            rangeFinderTime4 = rangeFinderRebuildTime4 + rangeFinderQueryTime4;
        }

        // Test 5: Clear and rebuild
        var clearTestParameters = GetParameters(characteristic, size / 5);
        var clearTestRanges = Gen.GenerateRanges<double>(clearTestParameters);
        
        // Measure IntervalTree clear and rebuild operations
        sw.Restart();
        intervalTree.Clear();
        PopulateIntervalTree(intervalTree, clearTestRanges);
        var intervalTreeClearRebuildTime = sw.Elapsed.TotalMilliseconds;
        
        // Measure RangeFinder performing identical clear + rebuild operations (rebuild with same final state)
        sw.Restart();
        currentRanges.Clear();
        currentRanges.AddRange(clearTestRanges);
        rangeFinder = new RangeFinder<double, int>(currentRanges);
        var rangeFinderRebuildTime5 = sw.Elapsed.TotalMilliseconds;
        
        TotalTests += 2;
        var (intervalTreeQueryTime5, rangeFinderQueryTime5) = ValidateQueriesHelperWithTiming(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterClear");
        
        // Include operation costs in total time
        var intervalTreeTime5 = intervalTreeClearRebuildTime + intervalTreeQueryTime5;
        var rangeFinderTime5 = rangeFinderRebuildTime5 + rangeFinderQueryTime5;

        // Calculate total times and performance ratio
        var totalIntervalTreeTime = intervalTreeTime1 + intervalTreeTime2 + intervalTreeTime3 + intervalTreeTime4 + intervalTreeTime5;
        var totalRangeFinderTime = rangeFinderTime1 + rangeFinderTime2 + rangeFinderTime3 + rangeFinderTime4 + rangeFinderTime5;
        
        // Store performance data in result
        result.IntervalTreeQueryTime = totalIntervalTreeTime;
        result.RangeFinderQueryTime = totalRangeFinderTime;

        // Test 6: Verify Count and Values properties
        if (intervalTree.Count != currentRanges.Count)
        {
            errors.Add(new CompatibilityError
            {
                QueryType = "Count",
                Query = "Count validation",
                RangeFinderResult = new[] { currentRanges.Count },
                IntervalTreeResult = new[] { intervalTree.Count }
            });
        }

        var expectedValues = currentRanges.Select(r => r.Value).OrderBy(v => v).ToArray();
        var actualValues = intervalTree.Values.OrderBy(v => v).ToArray();
        if (!expectedValues.SequenceEqual(actualValues))
        {
            errors.Add(new CompatibilityError
            {
                QueryType = "Values",
                Query = "Values property validation",
                RangeFinderResult = expectedValues,
                IntervalTreeResult = actualValues
            });
        }

        TotalTests += 2;

        result.CompatibilityErrors = errors;
        if (errors.Any())
        {
            FailureCount++;
        }

        return result;
    }

    /// <summary>
    /// Runs a comprehensive test of the IntervalTree wrapper functionality.
    /// </summary>
    public TestResult RunBasicCompatibilityTest(Characteristic characteristic, int size, int queryCount = 100)
    {
        var result = new TestResult
        {
            Characteristic = characteristic,
            Size = size,
            QueryCount = queryCount
        };

        var parameters = GetParameters(characteristic, size);
        var ranges = Gen.GenerateRanges<double>(parameters);
        var queries = Gen.GenerateQueryRanges<double>(parameters, queryCount);
        var points = Gen.GenerateQueryPoints<double>(parameters, queryCount);

        // Create both implementations
        var rangeFinder = new RangeFinder<double, int>(ranges);
        var intervalTree = new RangeTreeAdapter<double, int>();
        
        PopulateIntervalTree(intervalTree, ranges);

        TotalTests += queryCount * 2;

        var errors = new List<CompatibilityError>();
        ValidateQueriesHelper(intervalTree, rangeFinder, queries, points, errors, "Basic");

        result.CompatibilityErrors = errors;
        if (errors.Any())
        {
            FailureCount++;
        }

        return result;
    }


    private Parameter GetParameters(Characteristic characteristic, int size) => characteristic switch
    {
        Characteristic.Uniform => RangeParameterFactory.Uniform(size),
        Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(size),
        Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(size),
        Characteristic.Clustered => RangeParameterFactory.Clustered(size),
        _ => throw new ArgumentException($"Unknown characteristic: {characteristic}")
    };

    /// <summary>
    /// Runs continuous testing of IntervalTree wrapper functionality.
    /// </summary>
    public void RunContinuousTest(Action<TestResult>? progressCallback = null)
    {
        var shouldContinue = true;
        while (shouldContinue)
        {
            var characteristic = _characteristics[_random.Next(_characteristics.Length)];
            var size = _random.Next(10_000, 50_000); // Moderate sizes for reliable performance testing
            
            // Alternate between basic and dynamic tests
            var useDynamicTest = _random.Next(2) == 0;
            
            var result = useDynamicTest 
                ? RunDynamicOperationsTest(characteristic, size, 200) // 200 operations for reliable testing
                : RunBasicCompatibilityTest(characteristic, size, 200); // 200 queries for reliable testing
            
            progressCallback?.Invoke(result);

            if (result.CompatibilityErrors.Any())
            {
                shouldContinue = false;
            }
        }
    }

    private static void PopulateIntervalTree(RangeTreeAdapter<double, int> intervalTree, IEnumerable<NumericRange<double, int>> ranges)
    {
        var rangeList = ranges.ToList();
        for (int i = 0; i < rangeList.Count; i++)
        {
            var range = rangeList[i];
            intervalTree.Add(range.Start, range.End, range.Value);
        }
    }

    private static void AddRangesToIntervalTree(RangeTreeAdapter<double, int> intervalTree, IEnumerable<NumericRange<double, int>> ranges)
    {
        var rangeList = ranges.ToList();
        for (int i = 0; i < rangeList.Count; i++)
        {
            var range = rangeList[i];
            intervalTree.Add(range.Start, range.End, range.Value);
        }
    }

    private static void RemoveValuesFromIntervalTree(RangeTreeAdapter<double, int> intervalTree, IEnumerable<int> values)
    {
        var valueList = values.ToList();
        for (int i = 0; i < valueList.Count; i++)
        {
            intervalTree.Remove(valueList[i]);
        }
    }

    private static void ValidateQueriesHelper(
        RangeTreeAdapter<double, int> intervalTree,
        RangeFinder<double, int> rangeFinder,
        IEnumerable<NumericRange<double, object>> queries,
        IEnumerable<double> points,
        List<CompatibilityError> errors,
        string testPhase)
    {
        // Validate range queries
        var queryList = queries.ToList();
        for (int i = 0; i < queryList.Count; i++)
        {
            var q = queryList[i];
            var rfResult = rangeFinder.QueryRanges(q.Start, q.End).Select(r => r.Value).OrderBy(v => v).ToArray();
            var itResult = intervalTree.Query(q.Start, q.End).OrderBy(v => v).ToArray();

            if (!rfResult.SequenceEqual(itResult))
            {
                errors.Add(new CompatibilityError
                {
                    QueryType = $"RangeQuery-{testPhase}",
                    Query = $"[{q.Start:F3}, {q.End:F3}]",
                    RangeFinderResult = rfResult,
                    IntervalTreeResult = itResult,
                    OnlyInRangeFinder = rfResult.Except(itResult).ToArray(),
                    OnlyInIntervalTree = itResult.Except(rfResult).ToArray()
                });
            }
        }

        // Validate point queries
        var pointList = points.ToList();
        for (int i = 0; i < pointList.Count; i++)
        {
            var p = pointList[i];
            var rfResult = rangeFinder.QueryRanges(p).Select(r => r.Value).OrderBy(v => v).ToArray();
            var itResult = intervalTree.Query(p).OrderBy(v => v).ToArray();

            if (!rfResult.SequenceEqual(itResult))
            {
                errors.Add(new CompatibilityError
                {
                    QueryType = $"PointQuery-{testPhase}",
                    Query = $"{p:F3}",
                    RangeFinderResult = rfResult,
                    IntervalTreeResult = itResult,
                    OnlyInRangeFinder = rfResult.Except(itResult).ToArray(),
                    OnlyInIntervalTree = itResult.Except(rfResult).ToArray()
                });
            }
        }
    }

    private static (double intervalTreeTime, double rangeFinderTime) ValidateQueriesHelperWithTiming(
        RangeTreeAdapter<double, int> intervalTree,
        RangeFinder<double, int> rangeFinder,
        IEnumerable<NumericRange<double, object>> queries,
        IEnumerable<double> points,
        List<CompatibilityError> errors,
        string testPhase)
    {
        var sw = Stopwatch.StartNew();
        
        // Time IntervalTree queries (includes lazy reconstruction cost)
        sw.Restart();
        var intervalTreeResults = new List<int[]>();
        
        // Range queries for IntervalTree
        var queryList = queries.ToList();
        for (int i = 0; i < queryList.Count; i++)
        {
            var q = queryList[i];
            var itResult = intervalTree.Query(q.Start, q.End).OrderBy(v => v).ToArray();
            intervalTreeResults.Add(itResult);
        }
        
        // Point queries for IntervalTree
        var pointList = points.ToList();
        for (int i = 0; i < pointList.Count; i++)
        {
            var p = pointList[i];
            var itResult = intervalTree.Query(p).OrderBy(v => v).ToArray();
            intervalTreeResults.Add(itResult);
        }
        
        var intervalTreeTime = sw.Elapsed.TotalMilliseconds;
        
        // Time RangeFinder queries (using pre-built instance, no reconstruction cost)
        sw.Restart();
        var rangeFinderResults = new List<int[]>();
        
        // Range queries for RangeFinder
        for (int i = 0; i < queryList.Count; i++)
        {
            var q = queryList[i];
            var rfResult = rangeFinder.QueryRanges(q.Start, q.End).Select(r => r.Value).OrderBy(v => v).ToArray();
            rangeFinderResults.Add(rfResult);
        }
        
        // Point queries for RangeFinder
        for (int i = 0; i < pointList.Count; i++)
        {
            var p = pointList[i];
            var rfResult = rangeFinder.QueryRanges(p).Select(r => r.Value).OrderBy(v => v).ToArray();
            rangeFinderResults.Add(rfResult);
        }
        
        var rangeFinderTime = sw.Elapsed.TotalMilliseconds;
        
        // Now validate results for correctness (same as original ValidateQueriesHelper)
        for (int i = 0; i < queryList.Count; i++)
        {
            var q = queryList[i];
            var rfResult = rangeFinderResults[i];
            var itResult = intervalTreeResults[i];

            if (!rfResult.SequenceEqual(itResult))
            {
                errors.Add(new CompatibilityError
                {
                    QueryType = $"RangeQuery-{testPhase}",
                    Query = $"[{q.Start:F3}, {q.End:F3}]",
                    RangeFinderResult = rfResult,
                    IntervalTreeResult = itResult,
                    OnlyInRangeFinder = rfResult.Except(itResult).ToArray(),
                    OnlyInIntervalTree = itResult.Except(rfResult).ToArray()
                });
            }
        }

        for (int i = 0; i < pointList.Count; i++)
        {
            var p = pointList[i];
            var rfResult = rangeFinderResults[queryList.Count + i];
            var itResult = intervalTreeResults[queryList.Count + i];

            if (!rfResult.SequenceEqual(itResult))
            {
                errors.Add(new CompatibilityError
                {
                    QueryType = $"PointQuery-{testPhase}",
                    Query = $"{p:F3}",
                    RangeFinderResult = rfResult,
                    IntervalTreeResult = itResult,
                    OnlyInRangeFinder = rfResult.Except(itResult).ToArray(),
                    OnlyInIntervalTree = itResult.Except(rfResult).ToArray()
                });
            }
        }
        
        return (intervalTreeTime, rangeFinderTime);
    }
}