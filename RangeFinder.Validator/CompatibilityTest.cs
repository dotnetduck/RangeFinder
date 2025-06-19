using RangeFinder.Core;
using RangeFinder.IO;
using IntervalTree;
using RangeFinder.IO.Generation;
using Gen = RangeFinder.IO.Generation.Generator;

namespace RangeFinder.Validator;

/// <summary>
/// Performs correctness validation between RangeFinder and IntervalTree implementations
/// to ensure identical query results across various dataset characteristics.
/// Focus: Result compatibility, not performance measurement.
/// </summary>
public class CompatibilityTest
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
    /// Runs a single correctness validation test with specified parameters.
    /// </summary>
    /// <param name="characteristic">Dataset characteristic to test</param>
    /// <param name="size">Number of ranges to generate</param>
    /// <param name="queryCount">Number of queries to execute</param>
    /// <returns>Test result with compatibility validation</returns>
    public TestResult RunTest(Characteristic characteristic, int size, int queryCount = 1000)
    {
        var result = new TestResult
        {
            Characteristic = characteristic,
            Size = size,
            QueryCount = queryCount
        };

        // Data generation
        var parameters = GetParameters(characteristic, size);
        var ranges = Gen.GenerateRanges<double>(parameters);
        var queries = Gen.GenerateQueryRanges<double>(parameters, queryCount);
        var points = Gen.GenerateQueryPoints<double>(parameters, queryCount);

        // RangeFinder construction
        var rf = new RangeFinder<double, int>(ranges);

        // IntervalTree construction
        var it = new IntervalTree<double, int>();
        ranges.ForEach(r => it.Add(r.Start, r.End, r.Value));

        // Count actual tests performed
        TotalTests += queryCount * 2; // range queries + point queries

        // Correctness validation
        result.CompatibilityErrors = ValidateCompatibility(rf, it, queries, points);

        if (result.CompatibilityErrors.Any())
        {
            FailureCount++;
        }

        return result;
    }

    /// <summary>
    /// Runs continuous correctness validation until a failure is found or stopped.
    /// </summary>
    /// <param name="progressCallback">Called periodically with progress updates</param>
    public void RunContinuousTest(Action<TestResult>? progressCallback = null)
    {
        while (true)
        {
            var characteristic = _characteristics[_random.Next(_characteristics.Length)];
            var size = _random.Next(0, 1_000_000);
            var queryCount = 1000;

            var result = RunTest(characteristic, size, queryCount);
            
            progressCallback?.Invoke(result);

            if (result.CompatibilityErrors.Any())
            {
                break;
            }
        }
    }

    private Parameter GetParameters(Characteristic characteristic, int size) => characteristic switch
    {
        Characteristic.Uniform => RangeParameterFactory.Uniform(size),
        Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(size),
        Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(size),
        Characteristic.Clustered => RangeParameterFactory.Clustered(size),
        _ => throw new ArgumentException($"Unknown characteristic: {characteristic}")
    };

    private List<CompatibilityError> ValidateCompatibility(
        RangeFinder<double, int> rf, 
        IntervalTree<double, int> it,
        IEnumerable<NumericRange<double, object>> queries,
        IEnumerable<double> points)
    {
        var errors = new List<CompatibilityError>();

        // Validate range queries
        foreach (var q in queries)
        {
            var rfResult = rf.Query(q.Start, q.End).OrderBy(v => v).ToArray();
            var itResult = it.Query(q.Start, q.End).OrderBy(v => v).ToArray();

            if (!rfResult.SequenceEqual(itResult))
            {
                var onlyInRF = rfResult.Except(itResult).ToArray();
                var onlyInIT = itResult.Except(rfResult).ToArray();

                errors.Add(new CompatibilityError
                {
                    QueryType = "RangeQuery",
                    Query = $"[{q.Start:F3}, {q.End:F3}]",
                    RangeFinderResult = rfResult,
                    IntervalTreeResult = itResult,
                    OnlyInRangeFinder = onlyInRF,
                    OnlyInIntervalTree = onlyInIT
                });
            }
        }

        // Validate point queries
        foreach (var p in points)
        {
            var rfResult = rf.Query(p).OrderBy(v => v).ToArray();
            var itResult = it.Query(p).OrderBy(v => v).ToArray();

            if (!rfResult.SequenceEqual(itResult))
            {
                var onlyInRF = rfResult.Except(itResult).ToArray();
                var onlyInIT = itResult.Except(rfResult).ToArray();

                errors.Add(new CompatibilityError
                {
                    QueryType = "PointQuery",
                    Query = $"{p:F3}",
                    RangeFinderResult = rfResult,
                    IntervalTreeResult = itResult,
                    OnlyInRangeFinder = onlyInRF,
                    OnlyInIntervalTree = onlyInIT
                });
            }
        }

        return errors;
    }
}