using RangeFinder.Core;
using RangeFinder.Generator;
using System.Reflection;

namespace RangeGeneratorTests;

/// <summary>
/// Tests for core dataset generation functionality.
/// Verifies that range generation produces valid datasets with expected statistical characteristics.
/// </summary>
[TestFixture]
public class DatasetGenerationTests
{
    [Test]
    public void RangeGenerator_Presets_GenerateExpectedCharacteristics()
    {
        const int testSize = 50_000; // Large dataset for robust characteristics validation
        
        // Test each preset generates data with expected characteristics
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
            var analysis = Analyzer.Analyze(ranges);
            
            Assert.That(analysis.Count, Is.EqualTo(testSize), $"{name}: Should generate exactly {testSize} ranges");
            Assert.That(analysis.MinValue, Is.GreaterThanOrEqualTo(0), 
                $"{name}: MinValue should be positive");
            Assert.That(analysis.MaxValue, Is.LessThanOrEqualTo(parameters.TotalSpace), 
                $"{name}: MaxValue should respect parameter bounds");
            Assert.That(analysis.AverageLength, Is.GreaterThan(0), 
                $"{name}: Should have positive average length");
            
            Console.WriteLine($"{name} Dataset Analysis:");
            Console.WriteLine($"  Count: {analysis.Count}");
            Console.WriteLine($"  Range: [{analysis.MinValue:F2}, {analysis.MaxValue:F2}]");
            Console.WriteLine($"  Avg Length: {analysis.AverageLength:F2} ± {analysis.LengthStdDev:F2}");
            Console.WriteLine($"  Avg Interval: {analysis.AverageInterval:F2} ± {analysis.IntervalStdDev:F2}");
            Console.WriteLine($"  Overlap %: {analysis.OverlapPercentage:F1}%");
            Console.WriteLine();
        }
    }

    [Test]
    public void RangeGenerator_IntegerRanges_GeneratesCorrectly()
    {
        var intParameters = RangeParameterFactory.Custom(
            count: 1000,
            spacePerRange: 10.0, // Each range gets 10 units on average
            lengthRatio: 1.0,    // Range fills its allocated space
            overlapFactor: 2.0,  // Moderate overlap
            lengthVariability: 0.25,
            clusteringFactor: 0.3
        );

        var ranges = Generator.GenerateRanges<int>(intParameters);

        Assert.That(ranges, Has.Count.EqualTo(1000));

        // Verify all ranges are valid integers
        foreach (var range in ranges)
        {
            Assert.That(range.Start, Is.GreaterThanOrEqualTo(0));
            Assert.That(range.End, Is.LessThanOrEqualTo(10000));
            Assert.That(range.Start, Is.LessThanOrEqualTo(range.End));
        }
    }

    [Test]
    public void RangeGenerator_RepeatableResults_WithSameSeed()
    {
        var parameters = RangeParameterFactory.Uniform(100);
        
        var ranges1 = Generator.GenerateRanges<double>(parameters);
        var ranges2 = Generator.GenerateRanges<double>(parameters);

        Assert.That(ranges1.Count, Is.EqualTo(ranges2.Count));
        
        for (int i = 0; i < ranges1.Count; i++)
        {
            Assert.That(ranges1[i].Start, Is.EqualTo(ranges2[i].Start).Within(1e-10));
            Assert.That(ranges1[i].End, Is.EqualTo(ranges2[i].End).Within(1e-10));
            Assert.That(ranges1[i].Value, Is.EqualTo(ranges2[i].Value));
        }
    }

    [Test]
    public void RangeGenerator_DifferentSeeds_ProduceDifferentResults()
    {
        var params1 = RangeParameterFactory.Custom(
            count: 100,
            spacePerRange: 5.0,
            lengthRatio: 1.0,
            overlapFactor: 2.5,
            lengthVariability: 0.2,
            clusteringFactor: 0.25) with { RandomSeed = 42 };

        var params2 = RangeParameterFactory.Custom(
            count: 100,
            spacePerRange: 5.0,
            lengthRatio: 1.0,
            overlapFactor: 2.5,
            lengthVariability: 0.2,
            clusteringFactor: 0.25) with { RandomSeed = 123 };
        
        var ranges1 = Generator.GenerateRanges<double>(params1);
        var ranges2 = Generator.GenerateRanges<double>(params2);

        // Results should be different with different seeds
        bool foundDifference = false;
        for (int i = 0; i < ranges1.Count && !foundDifference; i++)
        {
            if (Math.Abs(ranges1[i].Start - ranges2[i].Start) > 1e-10 ||
                Math.Abs(ranges1[i].End - ranges2[i].End) > 1e-10)
            {
                foundDifference = true;
            }
        }
        
        Assert.That(foundDifference, Is.True, "Different seeds should produce different datasets");
    }
    
    [Test]
    public void RangeGenerator_PerformanceRegression_CompletesWithinReasonableTime()
    {
        var parameters = RangeParameterFactory.Uniform(100_000); // Large dataset
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var ranges = Generator.GenerateRanges<double>(parameters);
        stopwatch.Stop();
        
        Assert.That(ranges, Has.Count.EqualTo(parameters.Count), "Performance test should generate correct count");
        
        var maxExpectedTime = TimeSpan.FromMilliseconds(Math.Max(1000, 100_000 / 100.0));
        Assert.That(stopwatch.Elapsed, Is.LessThan(maxExpectedTime), 
            $"Large dataset generation took {stopwatch.Elapsed.TotalMilliseconds:F1}ms, expected < {maxExpectedTime.TotalMilliseconds:F1}ms");
    }
    
    [Test]
    public void RangeGenerator_EmptyDataset_HandlesCorrectly()
    {
        // Edge case: count = 0 should be handled gracefully
        Assert.Throws<ArgumentException>(() => 
        {
            var parameters = RangeParameterFactory.Uniform(0);
            Generator.GenerateRanges<double>(parameters);
        }, "Zero count should be rejected during validation");
    }
    
    [Test]
    public void RangeGenerator_ExtremeParameters_HandleGracefully()
    {
        // Test with very small space allocation
        var tinyParams = RangeParameterFactory.Custom(
            count: 2,
            spacePerRange: 0.001,
            lengthRatio: 0.5,
            overlapFactor: 1.0,
            lengthVariability: 0.0,
            clusteringFactor: 0.0);
            
        var ranges = Generator.GenerateRanges<double>(tinyParams);
        Assert.That(ranges, Has.Count.EqualTo(tinyParams.Count), "Tiny space allocation should generate correct count");
        foreach (var range in ranges)
        {
            Assert.That(range.Start, Is.LessThanOrEqualTo(range.End), "Generated ranges should be valid");
        }
        
        // Test with very high overlap requirement
        var overlapParams = RangeParameterFactory.Custom(
            count: 10,
            spacePerRange: 1.0,
            lengthRatio: 2.0, // Ranges larger than allocated space
            overlapFactor: 5.0, // High overlap requirement
            lengthVariability: 0.1,
            clusteringFactor: 0.1);
            
        var overlapRanges = Generator.GenerateRanges<double>(overlapParams);
        Assert.That(overlapRanges, Has.Count.EqualTo(overlapParams.Count), "High overlap requirement should generate correct count");
        foreach (var range in overlapRanges)
        {
            Assert.That(range.Start, Is.LessThanOrEqualTo(range.End), "Generated ranges should be valid");
        }
    }
}