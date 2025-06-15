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
}