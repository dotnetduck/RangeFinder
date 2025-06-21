using RangeFinder.Core;
using RangeFinder.Tests.Helper;

namespace RangeFinder.Tests.Core;

/// <summary>
/// Tests for RangeFinder with negative values to ensure proper handling of negative ranges.
/// These tests complement property-based testing by providing deterministic coverage
/// of critical boundary conditions that random generation might miss consistently.
/// Property-based testing relies on random value generation and may not reliably
/// test negative value edge cases across all test runs.
/// </summary>
[TestFixture]
public class RangeFinderNegativeValueTests
{
    /// <summary>
    /// Test data with negative values to ensure proper handling of negative ranges.
    /// Added to complement property-based testing which relies on random generation
    /// and may not consistently cover critical boundary conditions like negative values.
    /// </summary>
    private static readonly List<NumericRange<double, int>> TestRangesWithNegatives = new()
    {
        new(-10.0, -5.0, 101),   // Completely negative range
        new(-3.0, 2.0, 102),     // Negative start, positive end (crosses zero)
        new(-1.0, 1.0, 103),     // Symmetric around zero
        new(0.0, 5.0, 104),      // Starting at zero
        new(3.0, 8.0, 105),      // Positive range
        new(-8.0, -2.0, 106),    // Another negative range
        new(-15.0, 15.0, 107)    // Wide range crossing zero
    };

    #region Range Query Tests with Negative Values
    
    [TestCase(-12.0, -1.0, new[] { 101, 102, 103, 106 })]  // Query covering multiple negative ranges
    [TestCase(-5.0, 0.0, new[] { 102, 103, 104 })]         // Query from negative to zero
    [TestCase(-2.0, 2.0, new[] { 102, 103, 104, 105 })]    // Query crossing zero
    [TestCase(0.0, 10.0, new[] { 104, 105 })]              // Query from zero to positive
    [TestCase(-20.0, 20.0, new[] { 101, 102, 103, 104, 105, 106, 107 })]  // Query covering all ranges
    [TestCase(-6.0, -6.0, new[] { 106 })]                  // Point query in negative range
    [TestCase(-100.0, -50.0, new int[0])]                  // Query outside all ranges (negative)
    public void Query_NegativeValues_ReturnsCorrectRanges(
        double queryStart, double queryEnd, int[] expectedValues)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        
        var actualRanges = rangeFinder.QueryRanges(queryStart, queryEnd).ToArray();
        var actualValues = actualRanges.Select(r => r.Value).ToArray();
        var difference = actualValues.CompareAsSets(expectedValues);
        
        if (!difference.AreEqual)
        {
            var rangeData = TestRangesWithNegatives.Select(r => (r.Start, r.End)).ToArray();
            difference.PrintRangeDebugInfo($"Range query [{queryStart}, {queryEnd}]", (queryStart, queryEnd), rangeData);
        }
        
        Assert.That(difference.AreEqual, Is.True, 
            $"Query [{queryStart}, {queryEnd}] failed. Expected: [{string.Join(", ", expectedValues)}]. {difference.GetDescription()}");
    }

    #endregion

    #region Point Query Tests with Negative Values

    [TestCase(-7.0, 2)]  // Point in ranges [-10.0,-5.0] and [-8.0,-2.0]
    [TestCase(-3.0, 2)]  // Point at boundary: ranges [-3.0,2.0] and [-8.0,-2.0]
    [TestCase(0.0, 3)]   // Point at zero: ranges [-3.0,2.0], [-1.0,1.0], [0.0,5.0]
    [TestCase(-1.0, 3)]  // Point in multiple ranges crossing zero
    [TestCase(-12.0, 0)] // Point outside all ranges (negative)
    public void Query_NegativePointValues_ReturnsCorrectCount(
        double point, int expectedCount)
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        
        var result = rangeFinder.QueryRanges(point).ToArray();
        
        Assert.That(result, Has.Length.EqualTo(expectedCount),
            $"Point query at {point} should return {expectedCount} ranges");
    }

    [Test]
    public void Query_NegativeValues_CorrectSpecificRanges()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        
        // Test point -7.0 which should be in ranges [-10.0,-5.0] and [-8.0,-2.0]
        var actualRanges = rangeFinder.QueryRanges(-7.0).ToArray();
        var expectedValues = new[] { 101, 106 }; // Values from ranges [-10.0,-5.0] and [-8.0,-2.0]
        
        var actualValues = actualRanges.Select(r => r.Value).ToArray();
        var difference = actualValues.CompareAsSets(expectedValues);
        
        if (!difference.AreEqual)
        {
            var rangeData = TestRangesWithNegatives.Select(r => (r.Start, r.End)).ToArray();
            difference.PrintRangeDebugInfo("Point query at -7.0", (-7.0, -7.0), rangeData);
        }
        
        Assert.That(difference.AreEqual, Is.True, 
            $"Point query at -7.0 failed. {difference.GetDescription()}");
    }

    #endregion

    #region Cross-Zero Boundary Tests

    [Test]
    public void Query_CrossingZero_HandlesNegativeToPositiveCorrectly()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        
        // Query that crosses zero should find ranges that overlap with the crossing region
        var actualRanges = rangeFinder.QueryRanges(-0.5, 0.5).ToArray();
        var expectedValues = new[] { 102, 103, 104 }; // Ranges that contain values in [-0.5, 0.5]
        
        var actualValues = actualRanges.Select(r => r.Value).ToArray();
        var difference = actualValues.CompareAsSets(expectedValues);
        
        if (!difference.AreEqual)
        {
            var rangeData = TestRangesWithNegatives.Select(r => (r.Start, r.End)).ToArray();
            difference.PrintRangeDebugInfo("Cross-zero query [-0.5, 0.5]", (-0.5, 0.5), rangeData);
        }
        
        Assert.That(difference.AreEqual, Is.True, 
            $"Cross-zero query failed. {difference.GetDescription()}");
    }

    [Test]
    public void Query_PurelyNegativeRange_ExcludesPositiveRanges()
    {
        var rangeFinder = new RangeFinder<double, int>(TestRangesWithNegatives);
        
        // Query purely negative range should not include ranges that are purely positive
        var actualRanges = rangeFinder.QueryRanges(-9.0, -6.0).ToArray();
        var expectedValues = new[] { 101, 106 }; // Only ranges that overlap with [-9.0, -6.0]
        
        var actualValues = actualRanges.Select(r => r.Value).ToArray();
        var difference = actualValues.CompareAsSets(expectedValues);
        
        if (!difference.AreEqual)
        {
            var rangeData = TestRangesWithNegatives.Select(r => (r.Start, r.End)).ToArray();
            difference.PrintRangeDebugInfo("Purely negative query [-9.0, -6.0]", (-9.0, -6.0), rangeData);
        }
        
        Assert.That(difference.AreEqual, Is.True, 
            $"Purely negative query failed. {difference.GetDescription()}");
    }

    #endregion
}