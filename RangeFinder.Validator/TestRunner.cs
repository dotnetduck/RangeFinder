using RangeFinder.Generator;

namespace RangeFinder.Validator;

/// <summary>
/// Handles different test execution modes for correctness validation.
/// </summary>
public class TestRunner
{
    private readonly CompatibilityTest _tester;

    public TestRunner()
    {
        _tester = new CompatibilityTest();
    }

    public void RunContinuousTest()
    {
        Console.WriteLine("\n🔄 Running continuous correctness test...");
        Console.WriteLine("Press Ctrl+C to stop...\n");
        
        var testCount = 0;
        
        _tester.RunContinuousTest(result =>
        {
            testCount++;
            
            if (result.IsCompatible)
            {
                // Progress report every 10 tests
                if (testCount % 10 == 0)
                {
                    Console.WriteLine($"✅ Test #{testCount}: {result.Characteristic} ({result.Size:N0} ranges) - Compatible");
                }
            }
            else
            {
                Console.WriteLine($"\n❌ CORRECTNESS FAILURE at test #{testCount}!");
                result.PrintSummary();
                result.PrintDetailedErrors();
            }
        });
    }
}