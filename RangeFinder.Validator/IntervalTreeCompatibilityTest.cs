using RangeFinder.Core;
using RangeFinder.Generator;
using IntervalTree;
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
        ValidateQueriesHelper(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "Initial");

        // Test 2: Add operations with dynamic rebuilding
        var newParameters = GetParameters(characteristic, size / 10);
        var newRanges = Gen.GenerateRanges<double>(newParameters);
        AddRangesToIntervalTree(intervalTree, newRanges);

        // Update reference with new ranges
        var allRanges = initialRanges.Concat(newRanges);
        rangeFinder = new RangeFinder<double, int>(allRanges);

        TotalTests += 2;
        ValidateQueriesHelper(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterAdd");

        // Test 3: Remove operations
        var rangeListCopy = allRanges.ToList();
        var toRemove = rangeListCopy.Take(size / 20).Select(r => r.Value).ToList();
        
        RemoveValuesFromIntervalTree(intervalTree, toRemove);
        rangeListCopy.RemoveAll(r => toRemove.Contains(r.Value));

        rangeFinder = new RangeFinder<double, int>(rangeListCopy);

        TotalTests += 2;
        ValidateQueriesHelper(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterRemove");

        // Test 4: Bulk remove operations
        if (rangeListCopy.Count > 5)
        {
            var bulkRemove = rangeListCopy.Take(3).Select(r => r.Value).ToList();
            intervalTree.Remove(bulkRemove);
            rangeListCopy.RemoveAll(r => bulkRemove.Contains(r.Value));
            
            rangeFinder = new RangeFinder<double, int>(rangeListCopy);
            
            TotalTests += 2;
            ValidateQueriesHelper(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterBulkRemove");
        }

        // Test 5: Clear and rebuild
        intervalTree.Clear();
        var clearTestParameters = GetParameters(characteristic, size / 5);
        var clearTestRanges = Gen.GenerateRanges<double>(clearTestParameters);
        PopulateIntervalTree(intervalTree, clearTestRanges);
        
        rangeFinder = new RangeFinder<double, int>(clearTestRanges);
        
        TotalTests += 2;
        ValidateQueriesHelper(intervalTree, rangeFinder, queries.Take(5), points.Take(5), errors, "AfterClear");

        // Test 6: Verify Count and Values properties
        if (intervalTree.Count != clearTestRanges.Count)
        {
            errors.Add(new CompatibilityError
            {
                QueryType = "Count",
                Query = "Count validation",
                RangeFinderResult = new[] { clearTestRanges.Count },
                IntervalTreeResult = new[] { intervalTree.Count }
            });
        }

        var expectedValues = clearTestRanges.Select(r => r.Value).OrderBy(v => v).ToArray();
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
            var size = _random.Next(10, 10_000); // Smaller sizes for dynamic operations
            
            // Alternate between basic and dynamic tests
            var useDynamicTest = _random.Next(2) == 0;
            
            var result = useDynamicTest 
                ? RunDynamicOperationsTest(characteristic, size)
                : RunBasicCompatibilityTest(characteristic, size);
            
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
}