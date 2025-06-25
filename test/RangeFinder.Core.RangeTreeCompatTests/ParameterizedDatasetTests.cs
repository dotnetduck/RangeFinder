using IntervalTree;
using RangeFinder.Core;
using RangeFinder.Serialization.Generation;
using Gen = RangeFinder.Serialization.Generation.Generator;

namespace RangeFinder.Core.RangeTreeCompatTests;

/// <summary>
/// Parameterized dataset tests demonstrating systematic performance validation
/// across diverse data characteristics using the Generator.
/// </summary>
[TestFixture]
public class ParameterizedDatasetTests
{

    #region Compatibility Tests with Parameterized Datasets

    [TestCase(Characteristic.DenseOverlapping, 1_000)]
    [TestCase(Characteristic.SparseNonOverlapping, 1_000)]
    [TestCase(Characteristic.Clustered, 1_000)]
    [TestCase(Characteristic.Uniform, 1_000)]
    public void RangeFinderVsIntervalTree_ParameterizedDataset_IdenticalResults(Characteristic presetType, int datasetSize)
    {
        // Get preset parameters
        var parameters = presetType switch
        {
            Characteristic.DenseOverlapping => RangeParameterFactory.DenseOverlapping(datasetSize),
            Characteristic.SparseNonOverlapping => RangeParameterFactory.SparseNonOverlapping(datasetSize),
            Characteristic.Clustered => RangeParameterFactory.Clustered(datasetSize),
            Characteristic.Uniform => RangeParameterFactory.Uniform(datasetSize),
            _ => throw new ArgumentException($"Unknown preset: {presetType}")
        };

        // Generate dataset
        var ranges = Gen.GenerateRanges<double>(parameters);
        var queryRanges = Gen.GenerateQueryRanges<double>(parameters, 50);

        // Setup implementations
        var rangeFinder = new RangeFinder<double, int>(ranges);
        var intervalTree = new IntervalTree<double, int>();
        foreach (var range in ranges)
        {
            intervalTree.Add(range.Start, range.End, range.Value);
        }

        // Test range queries
        foreach (var queryRange in queryRanges)
        {
            var rfResults = rangeFinder.QueryRanges(queryRange.Start, queryRange.End)
                .Select(r => r.Value)
                .OrderBy(v => v)
                .ToArray();

            var itResults = intervalTree.Query(queryRange.Start, queryRange.End)
                .OrderBy(v => v)
                .ToArray();

            Assert.That(rfResults.SequenceEqual(itResults), Is.True,
                $"{presetType}: Query [{queryRange.Start:F2}, {queryRange.End:F2}] should produce identical results");
        }

        // Test point queries
        var queryPoints = Gen.GenerateQueryPoints<double>(parameters, 25);
        foreach (var point in queryPoints)
        {
            var rfResults = rangeFinder.QueryRanges(point)
                .Select(r => r.Value)
                .OrderBy(v => v)
                .ToArray();

            var itResults = intervalTree.Query(point)
                .OrderBy(v => v)
                .ToArray();

            Assert.That(rfResults.SequenceEqual(itResults), Is.True,
                $"{presetType}: Point query at {point:F2} should produce identical results");
        }
    }

    #endregion


}
