using RangeFinder.Core;
using RangeFinder.TestUtilities.Generation;

namespace RangeFinder.Serialization.Tests;

/// <summary>
/// Tests for dataset analysis functionality.
/// Verifies that dataset statistical analysis produces correct results for various scenarios.
/// </summary>
[TestFixture]
public class DatasetAnalysisTests
{
    [Test]
    public void Generator_DatasetAnalysis_EmptyDataset_HandlesCorrectly()
    {
        var emptyRanges = new List<NumericRange<double, int>>();
        var analysis = Generator.AnalyzeDataset(emptyRanges);

        Assert.That(analysis.Count, Is.EqualTo(0));
        Assert.That(analysis.MinValue, Is.EqualTo(0));
        Assert.That(analysis.MaxValue, Is.EqualTo(0));
        Assert.That(analysis.AverageLength, Is.EqualTo(0));
        Assert.That(analysis.OverlapPercentage, Is.EqualTo(0));
    }

    [Test]
    public void Generator_DatasetAnalysis_SingleRange_HandlesCorrectly()
    {
        var singleRange = new List<NumericRange<double, int>>
        {
            new(1.0, 5.0, 0)
        };
        var analysis = Generator.AnalyzeDataset(singleRange);

        Assert.That(analysis.Count, Is.EqualTo(1));
        Assert.That(analysis.MinValue, Is.EqualTo(1.0));
        Assert.That(analysis.MaxValue, Is.EqualTo(5.0));
        Assert.That(analysis.AverageLength, Is.EqualTo(4.0));
        Assert.That(analysis.OverlapPercentage, Is.EqualTo(0)); // No pairs to overlap
    }

    [Test]
    public void Generator_DatasetAnalysis_TwoNonOverlappingRanges_CalculatesCorrectly()
    {
        var ranges = new List<NumericRange<double, int>>
        {
            new(1.0, 3.0, 0), // Length: 2.0
            new(5.0, 9.0, 1)  // Length: 4.0
        };
        var analysis = Generator.AnalyzeDataset(ranges);

        Assert.That(analysis.Count, Is.EqualTo(2));
        Assert.That(analysis.MinValue, Is.EqualTo(1.0));
        Assert.That(analysis.MaxValue, Is.EqualTo(9.0));
        Assert.That(analysis.AverageLength, Is.EqualTo(3.0)); // (2.0 + 4.0) / 2
        Assert.That(analysis.OverlapPercentage, Is.EqualTo(0)); // No overlap
    }

    [Test]
    public void Generator_DatasetAnalysis_TwoOverlappingRanges_CalculatesCorrectly()
    {
        var ranges = new List<NumericRange<double, int>>
        {
            new(1.0, 5.0, 0), // Length: 4.0
            new(3.0, 7.0, 1)  // Length: 4.0, overlaps [3.0, 5.0]
        };
        var analysis = Generator.AnalyzeDataset(ranges);

        Assert.That(analysis.Count, Is.EqualTo(2));
        Assert.That(analysis.MinValue, Is.EqualTo(1.0));
        Assert.That(analysis.MaxValue, Is.EqualTo(7.0));
        Assert.That(analysis.AverageLength, Is.EqualTo(4.0));
        Assert.That(analysis.OverlapPercentage, Is.GreaterThan(0)); // Should detect overlap
    }

    [Test]
    public void Generator_DatasetAnalysis_MultipleRanges_CalculatesStatisticsCorrectly()
    {
        var ranges = new List<NumericRange<double, int>>
        {
            new(0.0, 2.0, 0),  // Length: 2.0
            new(1.0, 4.0, 1),  // Length: 3.0
            new(3.0, 5.0, 2),  // Length: 2.0
            new(6.0, 10.0, 3), // Length: 4.0
            new(8.0, 12.0, 4)  // Length: 4.0
        };
        var analysis = Generator.AnalyzeDataset(ranges);

        Assert.That(analysis.Count, Is.EqualTo(5));
        Assert.That(analysis.MinValue, Is.EqualTo(0.0));
        Assert.That(analysis.MaxValue, Is.EqualTo(12.0));

        // Average length: (2 + 3 + 2 + 4 + 4) / 5 = 3.0
        Assert.That(analysis.AverageLength, Is.EqualTo(3.0));

        // Should have some overlap (ranges 0-1-2 and ranges 3-4)
        Assert.That(analysis.OverlapPercentage, Is.GreaterThan(0));

        // Standard deviation should be positive for varied lengths
        Assert.That(analysis.LengthStdDev, Is.GreaterThan(0));
    }

    [Test]
    public void Generator_DatasetAnalysis_IdenticalLengthRanges_HasZeroStandardDeviation()
    {
        var ranges = new List<NumericRange<double, int>>
        {
            new(0.0, 3.0, 0),  // Length: 3.0
            new(5.0, 8.0, 1),  // Length: 3.0
            new(10.0, 13.0, 2) // Length: 3.0
        };
        var analysis = Generator.AnalyzeDataset(ranges);

        Assert.That(analysis.Count, Is.EqualTo(3));
        Assert.That(analysis.AverageLength, Is.EqualTo(3.0));
        Assert.That(analysis.LengthStdDev, Is.EqualTo(0.0).Within(1e-10)); // Should be exactly 0
        Assert.That(analysis.OverlapPercentage, Is.EqualTo(0)); // No overlap
    }

    [Test]
    public void Generator_DatasetAnalysis_IntegerRanges_WorksCorrectly()
    {
        var ranges = new List<NumericRange<int, int>>
        {
            new(1, 5, 0),   // Length: 4
            new(3, 8, 1),   // Length: 5, overlaps with first
            new(10, 15, 2)  // Length: 5, no overlap
        };
        var analysis = Generator.AnalyzeDataset(ranges);

        Assert.That(analysis.Count, Is.EqualTo(3));
        Assert.That(analysis.MinValue, Is.EqualTo(1.0));
        Assert.That(analysis.MaxValue, Is.EqualTo(15.0));
        Assert.That(analysis.AverageLength, Is.EqualTo(14.0 / 3.0).Within(1e-10)); // (4 + 5 + 5) / 3
        Assert.That(analysis.OverlapPercentage, Is.GreaterThan(0)); // Should detect overlap between first two
    }

    [Test]
    public void Generator_DatasetAnalysis_GeneratedDatasets_ProduceReasonableStatistics()
    {
        const int testSize = 100;

        var presets = new[]
        {
            ("Dense", RangeParameterFactory.DenseOverlapping(testSize)),
            ("Sparse", RangeParameterFactory.SparseNonOverlapping(testSize)),
            ("Clustered", RangeParameterFactory.Clustered(testSize)),
            ("Uniform", RangeParameterFactory.Uniform(testSize))
        };

        foreach (var (name, parameters) in presets)
        {
            var ranges = Generator.GenerateRanges<double>(parameters);
            var analysis = Generator.AnalyzeDataset(ranges);

            // Basic sanity checks for all datasets
            Assert.That(analysis.Count, Is.EqualTo(testSize),
                $"{name}: Should analyze exactly {testSize} ranges");
            Assert.That(analysis.MinValue, Is.GreaterThanOrEqualTo(0),
                $"{name}: MinValue should be non-negative");
            Assert.That(analysis.MaxValue, Is.LessThanOrEqualTo(parameters.TotalSpace),
                $"{name}: MaxValue should be within total space");
            Assert.That(analysis.AverageLength, Is.GreaterThan(0),
                $"{name}: Should have positive average length");
            Assert.That(analysis.LengthStdDev, Is.GreaterThanOrEqualTo(0),
                $"{name}: Standard deviation should be non-negative");
            Assert.That(analysis.OverlapPercentage, Is.GreaterThanOrEqualTo(0),
                $"{name}: Overlap percentage should be non-negative");

            // Specific checks based on dataset characteristics
            switch (name)
            {
                case "Dense":
                    Assert.That(analysis.OverlapPercentage, Is.GreaterThan(100),
                        "Dense dataset should have high overlap (>100%)");
                    break;
                case "Sparse":
                    Assert.That(analysis.OverlapPercentage, Is.LessThan(50),
                        "Sparse dataset should have lower overlap than dense");
                    break;
            }
        }
    }

    [Test]
    public void Analyzer_ComplexOverlapScenarios_CalculatesCorrectly()
    {
        // Test fully overlapping ranges
        var fullyOverlapping = new List<NumericRange<double, int>>
        {
            new(0.0, 10.0, 0),
            new(0.0, 10.0, 1),  // Identical ranges
            new(0.0, 10.0, 2)
        };
        var fullOverlapAnalysis = Generator.AnalyzeDataset(fullyOverlapping);

        Assert.That(fullOverlapAnalysis.Count, Is.EqualTo(3), "Should count all ranges");
        Assert.That(fullOverlapAnalysis.AverageLength, Is.EqualTo(10.0), "Average length should be correct");
        Assert.That(fullOverlapAnalysis.LengthStdDev, Is.EqualTo(0.0).Within(1e-10), "Identical lengths should have zero std dev");
        Assert.That(fullOverlapAnalysis.OverlapPercentage, Is.GreaterThanOrEqualTo(200), "Fully overlapping should have very high overlap percentage (>=200%)");

        // Test partially overlapping ranges
        var partiallyOverlapping = new List<NumericRange<double, int>>
        {
            new(0.0, 5.0, 0),   // Length: 5
            new(3.0, 8.0, 1),   // Length: 5, overlaps [3,5] with first
            new(10.0, 15.0, 2)  // Length: 5, no overlap
        };
        var partialOverlapAnalysis = Generator.AnalyzeDataset(partiallyOverlapping);

        Assert.That(partialOverlapAnalysis.Count, Is.EqualTo(3), "Should count all ranges");
        Assert.That(partialOverlapAnalysis.AverageLength, Is.EqualTo(5.0), "Average length should be correct");
        Assert.That(partialOverlapAnalysis.OverlapPercentage, Is.GreaterThan(0).And.LessThan(100),
            "Partial overlap should be between 0 and 100%");
    }

    [Test]
    public void Analyzer_EdgeCaseRanges_HandleCorrectly()
    {
        // Test zero-length ranges
        var zeroLengthRanges = new List<NumericRange<double, int>>
        {
            new(5.0, 5.0, 0),  // Zero length
            new(10.0, 10.0, 1) // Zero length
        };
        var zeroLengthAnalysis = Generator.AnalyzeDataset(zeroLengthRanges);

        Assert.That(zeroLengthAnalysis.Count, Is.EqualTo(2), "Should count zero-length ranges");
        Assert.That(zeroLengthAnalysis.AverageLength, Is.EqualTo(0.0), "Zero-length ranges should have zero average");
        Assert.That(zeroLengthAnalysis.OverlapPercentage, Is.EqualTo(0), "Zero-length ranges should have no overlap");

        // Test touching ranges (adjacent, not overlapping)
        var touchingRanges = new List<NumericRange<double, int>>
        {
            new(0.0, 5.0, 0),
            new(5.0, 10.0, 1),  // Touches but doesn't overlap
            new(10.0, 15.0, 2)
        };
        var touchingAnalysis = Generator.AnalyzeDataset(touchingRanges);

        Assert.That(touchingAnalysis.Count, Is.EqualTo(3), "Should count touching ranges");
        Assert.That(touchingAnalysis.AverageLength, Is.EqualTo(5.0), "Average length should be correct");
        // Note: depending on overlap calculation method, touching might or might not count as overlap
    }
}
