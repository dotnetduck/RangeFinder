using RangeFinder.Core;
using RangeFinder.IO;
using System.Numerics;

namespace RangeFinder.Tests;

/// <summary>
/// Base class providing common test utilities and parameterized test data for RangeGenerator tests.
/// Eliminates code duplication and provides consistent test patterns across all test files.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Standard test dataset sizes for consistent testing across all scenarios.
    /// </summary>
    protected static class TestSizes
    {
        public const int Small = 100;
        public const int Medium = 1_000;
        public const int Large = 10_000;
        public const int Performance = 100_000; // Only for performance validation tests
    }

    /// <summary>
    /// Provides test cases for all characteristic types with reasonable sizes.
    /// </summary>
    protected static IEnumerable<TestCaseData> AllCharacteristicTestCases(int size = TestSizes.Small)
    {
        yield return new TestCaseData(Characteristic.DenseOverlapping, RangeParameterFactory.DenseOverlapping(size)).SetName($"DenseOverlapping_{size}");
        yield return new TestCaseData(Characteristic.SparseNonOverlapping, RangeParameterFactory.SparseNonOverlapping(size)).SetName($"SparseNonOverlapping_{size}");
        yield return new TestCaseData(Characteristic.Clustered, RangeParameterFactory.Clustered(size)).SetName($"Clustered_{size}");
        yield return new TestCaseData(Characteristic.Uniform, RangeParameterFactory.Uniform(size)).SetName($"Uniform_{size}");
    }

    /// <summary>
    /// Provides test cases for numeric types that are commonly used.
    /// </summary>
    protected static IEnumerable<TestCaseData> NumericTypeTestCases()
    {
        // Floating point types
        yield return new TestCaseData(typeof(double)).SetName("Double");
        yield return new TestCaseData(typeof(float)).SetName("Float");
        yield return new TestCaseData(typeof(decimal)).SetName("Decimal");
        
        // Signed integer types
        yield return new TestCaseData(typeof(int)).SetName("Int32");
        yield return new TestCaseData(typeof(long)).SetName("Int64");
        yield return new TestCaseData(typeof(short)).SetName("Int16");
        yield return new TestCaseData(typeof(sbyte)).SetName("SByte");
        
        // Unsigned integer types
        yield return new TestCaseData(typeof(uint)).SetName("UInt32");
        yield return new TestCaseData(typeof(ulong)).SetName("UInt64");
        yield return new TestCaseData(typeof(ushort)).SetName("UInt16");
        yield return new TestCaseData(typeof(byte)).SetName("Byte");
    }

    /// <summary>
    /// Helper class containing common validation methods to avoid repetitive assertion patterns.
    /// </summary>
    protected static class Validators
    {
        /// <summary>
        /// Validates that a range collection is well-formed and within expected bounds.
        /// </summary>
        public static void ValidateRangeCollection<TNumber>(
            ICollection<NumericRange<TNumber, int>> ranges, 
            Parameter parameters, 
            string context = "")
            where TNumber : INumber<TNumber>
        {
            var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";
            
            Assert.That(ranges, Is.Not.Null, $"{prefix}Range collection should not be null");
            Assert.That(ranges.Count, Is.EqualTo(parameters.Count), 
                $"{prefix}Should generate exactly {parameters.Count} ranges");

            foreach (var range in ranges)
            {
                Assert.That(range.Start, Is.LessThanOrEqualTo(range.End), 
                    $"{prefix}Range start should be <= end");
                
                // Convert to double for comparison with parameter bounds
                var startDouble = Convert.ToDouble(range.Start);
                var endDouble = Convert.ToDouble(range.End);
                
                Assert.That(startDouble, Is.GreaterThanOrEqualTo(0), 
                    $"{prefix}Range start should be non-negative");
                Assert.That(endDouble, Is.LessThanOrEqualTo(parameters.TotalSpace), 
                    $"{prefix}Range end should be within total space bounds");
            }
        }

        /// <summary>
        /// Validates that query ranges are well-formed and within dataset bounds.
        /// </summary>
        public static void ValidateQueryRanges<TNumber>(
            ICollection<NumericRange<TNumber, object>> queryRanges, 
            Parameter parameters, 
            int expectedCount,
            string context = "")
            where TNumber : INumber<TNumber>
        {
            var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";
            
            Assert.That(queryRanges, Is.Not.Null, $"{prefix}Query range collection should not be null");
            Assert.That(queryRanges.Count, Is.EqualTo(expectedCount), 
                $"{prefix}Should generate exactly {expectedCount} query ranges");

            foreach (var query in queryRanges)
            {
                Assert.That(query.Start, Is.LessThanOrEqualTo(query.End), 
                    $"{prefix}Query range start should be <= end");
                
                var startDouble = Convert.ToDouble(query.Start);
                var endDouble = Convert.ToDouble(query.End);
                
                Assert.That(startDouble, Is.GreaterThanOrEqualTo(0), 
                    $"{prefix}Query range start should be non-negative");
                Assert.That(endDouble, Is.LessThanOrEqualTo(parameters.TotalSpace), 
                    $"{prefix}Query range end should be within total space bounds");
            }
        }

        /// <summary>
        /// Validates that query points are within dataset bounds.
        /// </summary>
        public static void ValidateQueryPoints<TNumber>(
            ICollection<TNumber> queryPoints, 
            Parameter parameters, 
            int expectedCount,
            string context = "")
            where TNumber : INumber<TNumber>
        {
            var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";
            
            Assert.That(queryPoints, Is.Not.Null, $"{prefix}Query point collection should not be null");
            Assert.That(queryPoints.Count, Is.EqualTo(expectedCount), 
                $"{prefix}Should generate exactly {expectedCount} query points");

            foreach (var point in queryPoints)
            {
                var pointDouble = Convert.ToDouble(point);
                
                Assert.That(pointDouble, Is.GreaterThanOrEqualTo(0), 
                    $"{prefix}Query point should be non-negative");
                Assert.That(pointDouble, Is.LessThanOrEqualTo(parameters.TotalSpace), 
                    $"{prefix}Query point should be within total space bounds");
            }
        }

        /// <summary>
        /// Validates that dataset statistics are reasonable for generated data.
        /// </summary>
        public static void ValidateStats(Stats analysis, Parameter parameters, string context = "")
        {
            var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";
            
            Assert.That(analysis.Count, Is.EqualTo(parameters.Count), 
                $"{prefix}Analysis count should match parameter count");
            Assert.That(analysis.MinValue, Is.GreaterThanOrEqualTo(0), 
                $"{prefix}MinValue should be non-negative");
            Assert.That(analysis.MaxValue, Is.LessThanOrEqualTo(parameters.TotalSpace), 
                $"{prefix}MaxValue should be within total space");
            Assert.That(analysis.AverageLength, Is.GreaterThan(0), 
                $"{prefix}AverageLength should be positive");
            Assert.That(analysis.LengthStdDev, Is.GreaterThanOrEqualTo(0), 
                $"{prefix}LengthStdDev should be non-negative");
            Assert.That(analysis.OverlapPercentage, Is.GreaterThanOrEqualTo(0), 
                $"{prefix}OverlapPercentage should be non-negative");
        }

        /// <summary>
        /// Validates characteristic-specific expectations for overlap patterns.
        /// </summary>
        public static void ValidateCharacteristicSpecificBehavior(Characteristic characteristic, Stats analysis, string context = "")
        {
            var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";
            
            switch (characteristic)
            {
                case Characteristic.DenseOverlapping:
                    Assert.That(analysis.OverlapPercentage, Is.GreaterThan(100), 
                        $"{prefix}Dense dataset should have high overlap (>100%)");
                    break;
                
                case Characteristic.SparseNonOverlapping:
                    Assert.That(analysis.OverlapPercentage, Is.LessThan(50), 
                        $"{prefix}Sparse dataset should have low overlap (<50%)");
                    break;
                
                
                case Characteristic.Clustered:
                    Assert.That(analysis.LengthStdDev, Is.GreaterThan(0), 
                        $"{prefix}Clustered dataset should have length variability");
                    Assert.That(analysis.IntervalStdDev, Is.GreaterThan(0), 
                        $"{prefix}Clustered dataset should have interval variability");
                    break;
                
                case Characteristic.Uniform:
                    // Uniform should be balanced - not too high or too low overlap
                    Assert.That(analysis.OverlapPercentage, Is.LessThan(100), 
                        $"{prefix}Uniform dataset should have moderate overlap");
                    break;
            }
        }
    }

    /// <summary>
    /// Helper class for creating test parameters with edge cases and boundary conditions.
    /// </summary>
    protected static class TestParameterFactory
    {
        /// <summary>
        /// Creates parameters for boundary testing with minimal valid values.
        /// </summary>
        public static Parameter MinimalValidParameters() => new()
        {
            Count = 1,
            SpacePerRange = 0.001,
            LengthRatio = 0.001,
            LengthVariability = 0.0,
            OverlapFactor = 0.001,
            ClusteringFactor = 0.0,
            StartOffset = 0.0,
            RandomSeed = 42
        };

        /// <summary>
        /// Creates parameters for boundary testing with maximum reasonable values.
        /// </summary>
        public static Parameter MaximalValidParameters() => new()
        {
            Count = 1000,
            SpacePerRange = 100.0,
            LengthRatio = 4.9, // Just under the 5.0 limit
            LengthVariability = 1.9, // Just under the 2.0 limit
            OverlapFactor = 99.0, // Just under the 100.0 limit
            ClusteringFactor = 1.9, // Just under the 2.0 limit
            StartOffset = 1.0,
            RandomSeed = 42
        };

        /// <summary>
        /// Creates parameters that should trigger validation errors.
        /// </summary>
        public static IEnumerable<(Parameter Parameters, string ExpectedError)> InvalidParameterCases()
        {
            yield return (new Parameter { Count = 0, SpacePerRange = 1.0, LengthRatio = 1.0, OverlapFactor = 1.0 }, 
                "Calculated TotalSpace must be a positive value");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 0, LengthRatio = 1.0, OverlapFactor = 1.0 }, 
                "Calculated TotalSpace must be a positive value");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 0, OverlapFactor = 1.0 }, 
                "LengthRatio must be between 0 and 5.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 6.0, OverlapFactor = 1.0 }, 
                "LengthRatio must be between 0 and 5.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, LengthVariability = -0.1, OverlapFactor = 1.0 }, 
                "LengthVariability must be between 0 and 2.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, LengthVariability = 2.1, OverlapFactor = 1.0 }, 
                "LengthVariability must be between 0 and 2.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, OverlapFactor = 0 }, 
                "OverlapFactor must be between 0 and 100.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, OverlapFactor = 101.0 }, 
                "OverlapFactor must be between 0 and 100.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, OverlapFactor = 1.0, ClusteringFactor = -0.1 }, 
                "ClusteringFactor must be between 0 and 2.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, OverlapFactor = 1.0, ClusteringFactor = 2.1 }, 
                "ClusteringFactor must be between 0 and 2.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, OverlapFactor = 1.0, StartOffset = -0.1 }, 
                "StartOffset must be between 0 and 1.0");
            
            yield return (new Parameter { Count = 100, SpacePerRange = 1.0, LengthRatio = 1.0, OverlapFactor = 1.0, StartOffset = 1.1 }, 
                "StartOffset must be between 0 and 1.0");
        }
    }

    /// <summary>
    /// Helper methods for performance measurement and validation.
    /// </summary>
    protected static class PerformanceHelpers
    {
        /// <summary>
        /// Executes an action and measures execution time, useful for performance regression testing.
        /// </summary>
        public static TimeSpan MeasureExecutionTime(Action action)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// Validates that generation completes within reasonable time limits.
        /// </summary>
        public static void ValidatePerformance(TimeSpan executionTime, int datasetSize, string operation)
        {
            // Generous performance bounds - adjust based on actual performance requirements
            var expectedMaxDuration = TimeSpan.FromMilliseconds(Math.Max(100, datasetSize / 10.0));
            
            Assert.That(executionTime, Is.LessThan(expectedMaxDuration), 
                $"{operation} for {datasetSize} elements took {executionTime.TotalMilliseconds:F1}ms, " +
                $"expected < {expectedMaxDuration.TotalMilliseconds:F1}ms");
        }
    }
}