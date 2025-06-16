using ConsoleAppFramework;
using RangeFinder.Generator;

namespace RangeFinder.Validator;

/// <summary>
/// CLI commands for RangeFinder correctness validation against IntervalTree.
/// Focus: Result compatibility validation, not performance measurement.
/// </summary>
public class Commands
{
    private readonly CompatibilityTest _tester = new();
    private readonly IntervalTreeCompatibilityTest _intervalTreeTester = new();

    /// <summary>
    /// Run a single correctness validation test.
    /// </summary>
    /// <param name="characteristic">Dataset characteristic to test (Uniform, DenseOverlapping, SparseNonOverlapping, Clustered)</param>
    /// <param name="size">Number of ranges to generate</param>
    /// <param name="queries">Number of queries to execute</param>
    [Command("single")]
    public void RunSingleTest(
        string characteristic = "Uniform",
        int size = 10000,
        int queries = 1000)
    {
        if (!Enum.TryParse<Characteristic>(characteristic, true, out var charEnum))
        {
            Console.WriteLine($"❌ Invalid characteristic: {characteristic}");
            Console.WriteLine("Valid options: Uniform, DenseOverlapping, SparseNonOverlapping, Clustered");
            return;
        }

        Console.WriteLine($"🔍 Running correctness validation: {charEnum} with {size:N0} ranges and {queries:N0} queries");
        
        var result = _tester.RunTest(charEnum, size, queries);
        result.PrintSummary();
        
        if (!result.IsCompatible)
        {
            result.PrintDetailedErrors();
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Run continuous correctness validation until failure or manual stop.
    /// </summary>
    /// <param name="maxTests">Maximum number of tests to run (0 = unlimited)</param>
    /// <param name="reportInterval">Progress report interval</param>
    [Command("continuous")]
    public void RunContinuousTest(int maxTests = 0, int reportInterval = 10)
    {
        Console.WriteLine("🔄 Running continuous correctness validation...");
        if (maxTests > 0)
            Console.WriteLine($"   Maximum tests: {maxTests:N0}");
        else
            Console.WriteLine("   Press Ctrl+C to stop...");
        Console.WriteLine();
        
        var testCount = 0;
        
        _tester.RunContinuousTest(result =>
        {
            testCount++;
            
            if (result.IsCompatible)
            {
                if (testCount % reportInterval == 0)
                {
                    Console.WriteLine($"✅ Test #{testCount}: {result.Characteristic} ({result.Size:N0} ranges) - Compatible");
                }
                
                if (maxTests > 0 && testCount >= maxTests)
                {
                    Console.WriteLine($"\n✅ Completed {testCount} tests successfully - 100% correctness maintained");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine($"\n❌ CORRECTNESS FAILURE at test #{testCount}!");
                result.PrintSummary();
                result.PrintDetailedErrors();
                Environment.Exit(1);
            }
        });
    }

    /// <summary>
    /// Run correctness validation across different dataset characteristics.
    /// </summary>
    /// <param name="sizes">Comma-separated list of dataset sizes to test</param>
    /// <param name="queries">Number of queries per test</param>
    [Command("validate")]
    public void RunValidationSuite(
        string sizes = "1000,10000,100000",
        int queries = 100)
    {
        Console.WriteLine("🔍 Running correctness validation across characteristics...\n");
        
        var characteristics = new[] 
        { 
            Characteristic.Uniform,
            Characteristic.DenseOverlapping,
            Characteristic.SparseNonOverlapping,
            Characteristic.Clustered
        };
        
        var testSizes = sizes.Split(',').Select(s => int.Parse(s.Trim())).ToArray();
        
        Console.WriteLine("| Characteristic                | Size      | Compatible |");
        Console.WriteLine("|-------------------------------|-----------|------------|");
        
        var hasFailures = false;
        
        foreach (var characteristic in characteristics)
        {
            foreach (var size in testSizes)
            {
                var result = _tester.RunTest(characteristic, size, queries);
                
                Console.WriteLine($"| {characteristic,-29} | {size,9:N0} | {(result.IsCompatible ? "✅" : "❌"),9} |");
                
                if (!result.IsCompatible)
                {
                    hasFailures = true;
                    Console.WriteLine($"   Errors: {result.CompatibilityErrors.Count}");
                }
            }
        }
        
        Console.WriteLine($"\nTotal tests completed: {_tester.TotalTests:N0}");
        Console.WriteLine($"Correctness failures: {_tester.FailureCount}");
        
        if (hasFailures)
        {
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Test the RangeTree compatibility wrapper with dynamic operations.
    /// </summary>
    /// <param name="characteristic">Dataset characteristic to test</param>
    /// <param name="size">Number of ranges to generate</param>
    /// <param name="operations">Number of dynamic operations to perform</param>
    [Command("wrapper")]
    public void TestRangeTreeWrapper(
        string characteristic = "Uniform",
        int size = 1000,
        int operations = 100)
    {
        if (!Enum.TryParse<Characteristic>(characteristic, true, out var charEnum))
        {
            Console.WriteLine($"❌ Invalid characteristic: {characteristic}");
            Console.WriteLine("Valid options: Uniform, DenseOverlapping, SparseNonOverlapping, Clustered");
            return;
        }

        Console.WriteLine($"🧪 Testing RangeTree wrapper: {charEnum} with {size:N0} ranges and {operations:N0} operations");
        
        var result = _intervalTreeTester.RunDynamicOperationsTest(charEnum, size, operations);
        result.PrintSummary();
        
        if (!result.IsCompatible)
        {
            result.PrintDetailedErrors();
            Environment.Exit(1);
        }
        
        Console.WriteLine("✅ RangeTree wrapper passed all dynamic operation tests");
    }

    /// <summary>
    /// Run continuous testing of the RangeTree compatibility wrapper.
    /// </summary>
    /// <param name="maxTests">Maximum number of tests to run (0 = unlimited)</param>
    /// <param name="reportInterval">Progress report interval</param>
    [Command("wrapper-continuous")]
    public void RunContinuousWrapperTest(int maxTests = 0, int reportInterval = 10)
    {
        Console.WriteLine("🔄 Running continuous RangeTree wrapper validation...");
        if (maxTests > 0)
            Console.WriteLine($"   Maximum tests: {maxTests:N0}");
        else
            Console.WriteLine("   Press Ctrl+C to stop...");
        Console.WriteLine();
        
        var testCount = 0;
        
        var performanceStats = new List<(double ratio, int size)>();
        
        _intervalTreeTester.RunContinuousTest(result =>
        {
            testCount++;
            
            // Collect performance statistics
            if (result.PerformanceRatio > 0)
            {
                performanceStats.Add((result.PerformanceRatio, result.Size));
            }
            
            if (result.IsCompatible)
            {
                if (testCount % reportInterval == 0)
                {
                    var perfSummary = "";
                    if (performanceStats.Any())
                    {
                        var avgRatio = performanceStats.Average(x => x.ratio);
                        perfSummary = $" (avg perf: {avgRatio:F2}x)";
                    }
                    Console.WriteLine($"✅ Test #{testCount}: {result.Characteristic} ({result.Size:N0} ranges) - Compatible{perfSummary}");
                }
                
                if (maxTests > 0 && testCount >= maxTests)
                {
                    // Print final performance statistics
                    if (performanceStats.Any())
                    {
                        var avgRatio = performanceStats.Average(x => x.ratio);
                        var minRatio = performanceStats.Min(x => x.ratio);
                        var maxRatio = performanceStats.Max(x => x.ratio);
                        Console.WriteLine($"\n📊 Performance Summary over {testCount} tests:");
                        Console.WriteLine($"   Average ratio: {avgRatio:F2}x (IntervalTree/RangeFinder)");
                        Console.WriteLine($"   Range: {minRatio:F2}x - {maxRatio:F2}x");
                    }
                    Console.WriteLine($"\n✅ Completed {testCount} wrapper tests successfully - 100% compatibility maintained");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine($"\n❌ WRAPPER COMPATIBILITY FAILURE at test #{testCount}!");
                result.PrintSummary();
                result.PrintDetailedErrors();
                Environment.Exit(1);
            }
        });
    }
}