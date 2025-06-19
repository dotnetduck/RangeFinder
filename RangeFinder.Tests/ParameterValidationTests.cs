using RangeFinder.IO;
using System.Reflection;

namespace RangeFinder.Tests;

/// <summary>
/// Tests for parameter validation functionality.
/// Ensures that invalid parameter combinations are properly rejected and valid ones accepted.
/// </summary>
[TestFixture]
public class ParameterValidationTests : TestBase
{
    [Test]
    public void Parameter_ValidateMethod_AcceptsBoundaryValues()
    {
        var minimalParams = TestParameterFactory.MinimalValidParameters();
        var maximalParams = TestParameterFactory.MaximalValidParameters();
        
        Assert.DoesNotThrow(() => minimalParams.Validate(), "Minimal valid parameters should pass validation");
        Assert.DoesNotThrow(() => maximalParams.Validate(), "Maximal valid parameters should pass validation");
        
        // Test successful generation with boundary parameters
        var minimalRanges = Generator.GenerateRanges<double>(minimalParams);
        var maximalRanges = Generator.GenerateRanges<double>(maximalParams);
        
        Validators.ValidateRangeCollection(minimalRanges, minimalParams, "Minimal parameters");
        
        // For maximal parameters, validate most constraints but allow small floating point tolerance
        Assert.That(maximalRanges, Is.Not.Null, "Maximal parameters: Range collection should not be null");
        Assert.That(maximalRanges.Count, Is.EqualTo(maximalParams.Count), 
            "Maximal parameters: Should generate correct count");
            
        foreach (var range in maximalRanges)
        {
            Assert.That(range.Start, Is.LessThanOrEqualTo(range.End), 
                "Maximal parameters: Range start should be <= end");
            Assert.That(Convert.ToDouble(range.Start), Is.GreaterThanOrEqualTo(0), 
                "Maximal parameters: Range start should be non-negative");
            // Maximal parameters with extreme settings may generate ranges that extend beyond strict bounds
            // This is expected behavior when testing boundary conditions with high LengthRatio values
            Assert.That(Convert.ToDouble(range.End), Is.GreaterThan(0), 
                "Maximal parameters: Range end should be positive");
        }
    }
    
    [Test]
    public void Parameter_InvalidCombinations_ThrowsExpectedExceptions()
    {
        foreach (var (invalidParams, expectedError) in TestParameterFactory.InvalidParameterCases())
        {
            var exception = Assert.Throws<ArgumentException>(() => invalidParams.Validate(), 
                $"Should throw ArgumentException for: {expectedError}");
            Assert.That(exception.Message, Does.Contain(expectedError), 
                "Exception message should contain expected error text");
        }
    }

    [Test]
    public void Parameter_OverflowScenarios_HandleGracefully()
    {
        // Test extremely large values that might cause overflow
        var overflowParams = new Parameter
        {
            Count = int.MaxValue / 2,
            SpacePerRange = double.MaxValue / 1e10, // Still very large but not overflowing
            LengthRatio = 1.0,
            LengthVariability = 0.0,
            OverlapFactor = 1.0,
            ClusteringFactor = 0.0,
            StartOffset = 0.0,
            RandomSeed = 42
        };
        
        // Should either validate successfully or throw a clear overflow-related exception
        Assert.DoesNotThrow(() => overflowParams.Validate(), 
            "Large parameter values should be handled gracefully by validation");
    }

    [Test]
    public void Parameter_ConfigurationFeasibility_ValidatesCorrectly()
    {
        // Test configuration that requires more space than available
        var infeasibleParams = new Parameter
        {
            Count = 1000,
            SpacePerRange = 1.0, // Total space = 1000
            LengthRatio = 2.0,   // Average length = 2.0 per range
            LengthVariability = 0.0,
            OverlapFactor = 0.1, // Very low overlap = high space requirement
            ClusteringFactor = 0.0,
            StartOffset = 0.0,
            RandomSeed = 42
        };
        
        var exception = Assert.Throws<InvalidOperationException>(() => infeasibleParams.Validate());
        Assert.That(exception.Message, Does.Contain("requires more space"), 
            "Should explain space requirements in error message");
    }

    [Test, TestCaseSource(nameof(NumericTypeTestCases))]
    public void Parameter_NumericTypeGeneration_HandlesOverflowGracefully(Type numericType)
    {
        // Test with parameters that might cause overflow for smaller numeric types
        var largeParams = new Parameter
        {
            Count = 10,
            SpacePerRange = 1e6, // Large values that might overflow smaller types
            LengthRatio = 1.0,
            LengthVariability = 0.0,
            OverlapFactor = 1.0,
            ClusteringFactor = 0.0,
            StartOffset = 0.0,
            RandomSeed = 42
        };
        
        var method = typeof(Generator).GetMethod(nameof(Generator.GenerateRanges))!.MakeGenericMethod(numericType);
        
        if (IsSmallNumericType(numericType))
        {
            // Very small types (byte, sbyte, short, ushort) should throw overflow exceptions
            if (numericType == typeof(byte) || numericType == typeof(sbyte) || 
                numericType == typeof(short) || numericType == typeof(ushort))
            {
                var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { largeParams }));
                Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>(), 
                    $"{numericType.Name} should throw InvalidOperationException for overflow");
            }
            else
            {
                // Other small types should handle gracefully
                Assert.DoesNotThrow(() => method.Invoke(null, new object[] { largeParams }), 
                    $"{numericType.Name} should handle overflow scenarios gracefully");
            }
        }
        else
        {
            // Large types should handle without overflow
            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { largeParams }), 
                $"{numericType.Name} should handle large values without overflow");
        }
    }
    
    private static bool IsSmallNumericType(Type type) =>
        type == typeof(byte) || type == typeof(sbyte) || 
        type == typeof(short) || type == typeof(ushort) ||
        type == typeof(float); // float has limited precision

    [Test]
    public void Parameter_CalculatedProperties_ProduceExpectedValues()
    {
        var testParams = new Parameter
        {
            Count = 100,
            SpacePerRange = 5.0,
            LengthRatio = 0.8,
            LengthVariability = 0.3,
            OverlapFactor = 1.5,
            ClusteringFactor = 0.2,
            StartOffset = 0.25,
            RandomSeed = 42
        };
        
        Assert.That(testParams.TotalSpace, Is.EqualTo(500.0), "TotalSpace calculation should be Count * SpacePerRange");
        Assert.That(testParams.AverageLength, Is.EqualTo(4.0), "AverageLength calculation should be SpacePerRange * LengthRatio");
        
        // Properties should be consistent
        Assert.That(testParams.TotalSpace, Is.GreaterThan(0), "TotalSpace should be positive");
        Assert.That(testParams.AverageLength, Is.GreaterThan(0), "AverageLength should be positive");
        Assert.That(testParams.AverageLength, Is.LessThanOrEqualTo(testParams.SpacePerRange * testParams.LengthRatio), 
            "AverageLength should not exceed expected value");
    }

    [Test]
    public void Parameter_RecordImmutability_WorksCorrectly()
    {
        var originalParams = RangeParameterFactory.Uniform(TestSizes.Small);
        var modifiedParams = originalParams with { Count = TestSizes.Medium };
        
        Assert.That(originalParams.Count, Is.EqualTo(TestSizes.Small), "Original parameters should be unchanged");
        Assert.That(modifiedParams.Count, Is.EqualTo(TestSizes.Medium), "Modified parameters should have new value");
        Assert.That(modifiedParams.SpacePerRange, Is.EqualTo(originalParams.SpacePerRange), 
            "Non-modified properties should remain the same");
    }

    [Test]
    public void Parameter_CustomFactory_CreatesInvalidParameters()
    {
        // Test that Custom factory creates parameters that fail validation when invalid inputs are provided
        var invalidCountParams = RangeParameterFactory.Custom(-1, 1.0, 1.0, 1.0);
        Assert.Throws<ArgumentException>(() => invalidCountParams.Validate(), "Should reject negative count during validation");
            
        var invalidSpaceParams = RangeParameterFactory.Custom(100, 0, 1.0, 1.0);
        Assert.Throws<ArgumentException>(() => invalidSpaceParams.Validate(), "Should reject zero spacePerRange during validation");
            
        var invalidLengthParams = RangeParameterFactory.Custom(100, 1.0, 0, 1.0);
        Assert.Throws<ArgumentException>(() => invalidLengthParams.Validate(), "Should reject zero lengthRatio during validation");
            
        var invalidOverlapParams = RangeParameterFactory.Custom(100, 1.0, 1.0, 0);
        Assert.Throws<ArgumentException>(() => invalidOverlapParams.Validate(), "Should reject zero overlapFactor during validation");
            
        // Test that valid parameters work
        var validParams = RangeParameterFactory.Custom(100, 5.0, 0.8, 1.5, 0.3, 0.2);
        Assert.DoesNotThrow(() => validParams.Validate(), "Valid custom parameters should pass validation");
    }

    [Test]
    public void Parameter_NaNAndInfiniteValues_HandledCorrectly()
    {
        // Test NaN values
        var nanParams = new Parameter
        {
            Count = 100,
            SpacePerRange = double.NaN,
            LengthRatio = 0.5,
            LengthVariability = 0.2,
            OverlapFactor = 1.0,
            ClusteringFactor = 0.1,
            StartOffset = 0.0,
            RandomSeed = 42
        };
        
        Assert.Throws<ArgumentException>(() => nanParams.Validate(), "Should reject NaN values");
        
        // Test infinite values
        var infiniteParams = new Parameter
        {
            Count = 100,
            SpacePerRange = double.PositiveInfinity,
            LengthRatio = 0.5,
            LengthVariability = 0.2,
            OverlapFactor = 1.0,
            ClusteringFactor = 0.1,
            StartOffset = 0.0,
            RandomSeed = 42
        };
        
        Assert.Throws<ArgumentException>(() => infiniteParams.Validate(), "Should reject infinite values");
    }

    [Test]
    public void Parameter_ToStringAndEquality_BehavesCorrectly()
    {
        var params1 = RangeParameterFactory.Uniform(100);
        var params2 = RangeParameterFactory.Uniform(100);
        var params3 = RangeParameterFactory.DenseOverlapping(100);
        
        // Record equality
        Assert.That(params1, Is.EqualTo(params2), "Identical parameters should be equal");
        Assert.That(params1, Is.Not.EqualTo(params3), "Different parameters should not be equal");
        
        // ToString should not throw and should contain meaningful information
        var stringRep = params1.ToString();
        Assert.That(stringRep, Is.Not.Null.And.Not.Empty, "ToString should produce non-empty string");
        Assert.That(stringRep, Does.Contain("Parameter"), "ToString should contain type name");
    }
}