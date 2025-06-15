using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using ConsoleAppFramework;
using RangeFinder.Benchmarks;

public class BenchmarkCommands
{
    /// <summary>
    /// Run a single RangeFinder benchmark
    /// </summary>
    /// <param name="test">Type of benchmark: Construction, RangeQuery, PointQuery, Allocation</param>
    /// <param name="size">Dataset size: Size10K, Size100K, Size1M, Size5M (default: Size100K)</param>
    /// <param name="characteristics">Dataset characteristic: Uniform, Dense, Sparse, Temporal (default: Uniform)</param>
    /// <param name="accuracy">Accuracy level: Quick (~30s), Balanced (~2min), Accurate (~5-15min) (default: Balanced)</param>
    /// <param name="queries">Number of queries to execute per benchmark (default: 25)</param>
    /// <param name="output">Output directory for organized results (default: results/)</param>
    /// <param name="timestamp">Add timestamp prefix to result files (default: true)</param>
    [Command("run-single")]
    public static void RunSingle(
        TestType test,
        DatasetSize size = DatasetSize.Size100K,
        DatasetCharacteristic characteristics = DatasetCharacteristic.Uniform,
        AccuracyLevel accuracy = AccuracyLevel.Balanced,
        int queries = 25,
        string output = "results/",
        bool timestamp = true)
    {
        Console.WriteLine("🔧 RangeFinder Single Benchmark");
        Console.WriteLine($"Test: {test.ToDisplayString()}");
        Console.WriteLine($"Size: {size.ToDisplayString()} ({size.ToElementCount():N0} ranges)");
        Console.WriteLine($"Characteristics: {characteristics.ToDisplayString()}");
        Console.WriteLine($"Accuracy: {accuracy.ToDisplayString()}");
        Console.WriteLine($"Queries: {queries}");
        Console.WriteLine($"Output: {output}");
        Console.WriteLine();

        if (queries <= 0)
        {
            Console.WriteLine("❌ Error: Queries must be positive");
            return;
        }

        try
        {
            RunSingleBenchmark(test, size, characteristics, accuracy, queries);
            
            // Organize results
            var timestampStr = timestamp ? DateTime.Now.ToString("yyyyMMdd_HHmmss") : "";
            OrganizeResultsBySize(output, timestampStr, test, size, characteristics);
            
            Console.WriteLine("✅ Single benchmark completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Benchmark failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Run a comprehensive suite of RangeFinder benchmarks
    /// </summary>
    /// <param name="accuracy">Accuracy level: Quick (~5min total), Balanced (~15min total), Accurate (~45min total) (default: Balanced)</param>
    /// <param name="sizes">Comma-separated dataset sizes: Size10K,Size100K,Size1M,Size5M (default: Size100K,Size1M)</param>
    /// <param name="tests">Comma-separated test types: Construction,RangeQuery,PointQuery,Allocation (default: all types)</param>
    /// <param name="characteristics">Comma-separated dataset characteristics: Uniform,Dense,Sparse,Temporal (default: Uniform,Dense,Sparse)</param>
    /// <param name="output">Output directory for organized results (default: results/)</param>
    /// <param name="delay">Delay between benchmarks in seconds for system stabilization (default: 5)</param>
    /// <param name="queries">Number of queries per benchmark (default: 100)</param>
    /// <param name="timestamp">Add timestamp prefix to result files to prevent overwrites (default: true)</param>
    /// <param name="summary">Generate comprehensive suite summary report (default: true)</param>
    [Command("run-suite")]
    public static void RunSuite(
        AccuracyLevel accuracy = AccuracyLevel.Balanced,
        string sizes = "Size100K,Size1M",
        string tests = "Construction,RangeQuery,PointQuery,Allocation",
        string characteristics = "Uniform,Dense,Sparse",
        string output = "results/",
        int delay = 5,
        int queries = 25,
        bool timestamp = true,
        bool summary = true)
    {
        Console.WriteLine("🚀 RangeFinder Benchmark Suite");
        Console.WriteLine($"Accuracy: {accuracy.ToDisplayString()}");
        Console.WriteLine($"Sizes: {sizes}");
        Console.WriteLine($"Tests: {tests}");
        Console.WriteLine($"Characteristics: {characteristics}");
        Console.WriteLine($"Output: {output}");
        Console.WriteLine($"Queries per benchmark: {queries}");
        Console.WriteLine($"Delay between benchmarks: {delay}s");
        Console.WriteLine($"Timestamp results: {timestamp}");
        Console.WriteLine($"Generate summary: {summary}");
        Console.WriteLine();
        Console.WriteLine("⚠️  IMPORTANT: Running benchmarks sequentially to avoid resource contention");
        Console.WriteLine("   Do not run other intensive processes during benchmarking");
        Console.WriteLine();

        // Validation
        if (queries <= 0)
        {
            Console.WriteLine("❌ Error: Queries must be positive");
            return;
        }

        if (delay < 0)
        {
            Console.WriteLine("❌ Error: Delay must be non-negative");
            return;
        }

        // Parse and validate parameters
        var sizeList = ParseSizes(sizes);
        var testList = ParseTests(tests);
        var characteristicList = ParseCharacteristics(characteristics);

        if (sizeList.Count == 0)
        {
            Console.WriteLine("❌ Error: No valid sizes specified");
            Console.WriteLine("Valid sizes: Size10K, Size100K, Size1M, Size5M");
            return;
        }

        if (testList.Count == 0)
        {
            Console.WriteLine("❌ Error: No valid tests specified");
            Console.WriteLine("Valid tests: Construction, RangeQuery, PointQuery, Allocation");
            return;
        }
        
        if (characteristicList.Count == 0)
        {
            Console.WriteLine("❌ Error: No valid characteristics specified");
            Console.WriteLine("Valid characteristics: Uniform, Dense, Sparse, Temporal");
            return;
        }

        // Create benchmark matrix - include dataset characteristics
        var benchmarks = new List<(TestType test, DatasetSize size, DatasetCharacteristic characteristic)>();
        foreach (var test in testList)
        {
            foreach (var size in sizeList)
            {
                foreach (var characteristic in characteristicList)
                {
                    benchmarks.Add((test, size, characteristic));
                }
            }
        }

        var total = benchmarks.Count;
        var timestampStr = timestamp ? DateTime.Now.ToString("yyyyMMdd_HHmmss") : "";

        Console.WriteLine($"📊 Running {total} benchmark configurations:");
        foreach (var (test, size, characteristic) in benchmarks)
        {
            Console.WriteLine($"   • {test.ToDisplayString()} ({size.ToDisplayString()}) [{characteristic.ToDisplayString()}]");
        }
        Console.WriteLine();

        // Execute benchmark suite
        var results = new List<(TestType test, DatasetSize size, DatasetCharacteristic characteristic, bool success, string error)>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < benchmarks.Count; i++)
        {
            var (test, size, characteristic) = benchmarks[i];
            var current = i + 1;
            
            Console.WriteLine($"📈 Progress: {current}/{total} - {test.ToDisplayString()} ({size.ToDisplayString()}) [{characteristic.ToDisplayString()}]");
            
            try
            {
                RunSingleBenchmark(test, size, characteristic, accuracy, queries);
                results.Add((test, size, characteristic, true, ""));
                Console.WriteLine($"✅ Completed {test.ToDisplayString()} ({size.ToDisplayString()}) [{characteristic.ToDisplayString()}]");
                
                // Immediately organize results after each benchmark to prevent overwrites
                OrganizeResultsBySize(output, timestampStr, test, size, characteristic);
            }
            catch (Exception ex)
            {
                results.Add((test, size, characteristic, false, ex.Message));
                Console.WriteLine($"❌ Failed {test.ToDisplayString()} ({size.ToDisplayString()}) [{characteristic.ToDisplayString()}]: {ex.Message}");
                Console.WriteLine("⏭️  Continuing with remaining benchmarks...");
            }

            // System stabilization delay
            if (current < total && delay > 0)
            {
                Console.WriteLine($"⏳ Waiting {delay}s for system stabilization...");
                Thread.Sleep(delay * 1000);
                Console.WriteLine();
            }
        }

        stopwatch.Stop();

        // Post-processing - no need to reorganize, already done after each benchmark

        if (summary)
        {
            Console.WriteLine("📝 Generating suite summary...");
            GenerateSuiteSummary(output, timestampStr, accuracy, results, stopwatch.Elapsed);
        }

        // Final report
        var successful = results.Count(r => r.success);
        var failed = results.Count(r => !r.success);

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("🏁 Benchmark Suite Complete");
        Console.WriteLine($"⏱️  Total time: {stopwatch.Elapsed:hh\\:mm\\:ss}");
        Console.WriteLine($"📊 Total configurations: {total}");
        Console.WriteLine($"✅ Successful: {successful}");
        Console.WriteLine($"❌ Failed: {failed}");
        
        if (failed == 0)
        {
            Console.WriteLine("🎉 All benchmarks completed successfully!");
        }
        else
        {
            Console.WriteLine($"⚠️  {failed} benchmark(s) failed:");
            foreach (var (test, size, characteristic, success, error) in results.Where(r => !r.success))
            {
                Console.WriteLine($"   • {test.ToDisplayString()} ({size.ToDisplayString()}) [{characteristic.ToDisplayString()}]: {error}");
            }
        }
        
        Console.WriteLine($"📂 Results available in: {Path.GetFullPath(output)}");
        Console.WriteLine("═══════════════════════════════════════════════════════");
    }

    /// <summary>
    /// Debug characteristics to verify they're working correctly
    /// </summary>
    [Command("debug-characteristics")]
    public static void DebugCharacteristics()
    {
        Console.WriteLine("🔍 Debugging Dataset Characteristics");
        Console.WriteLine();

        var characteristics = new[] { DatasetCharacteristic.Uniform, DatasetCharacteristic.Dense, DatasetCharacteristic.Sparse };
        var datasetSize = 1000;

        foreach (var characteristic in characteristics)
        {
            Console.WriteLine($"Testing {characteristic}:");
            
            // Set environment variables for the test
            Environment.SetEnvironmentVariable("BENCHMARK_DATASET_SIZE", datasetSize.ToString());
            Environment.SetEnvironmentVariable("BENCHMARK_CHARACTERISTIC", characteristic.ToString());
            
            var benchmark = new ConstructionBenchmarks();
            benchmark.Setup(); // This should call GenerateSourceData with the characteristic
            
            Console.WriteLine($"  Environment characteristic: {Environment.GetEnvironmentVariable("BENCHMARK_CHARACTERISTIC")}");
            Console.WriteLine();
        }
    }


    private static void RunSingleBenchmark(TestType test, DatasetSize size, DatasetCharacteristic characteristic, AccuracyLevel accuracy, int queries)
    {
        var datasetSize = size.ToElementCount();
        Console.WriteLine($"🔧 Configuring: DatasetSize={datasetSize:N0}, QueryCount={queries}, Characteristic={characteristic}");

        // Clear old artifacts to ensure clean results
        CleanArtifactsDirectory();

        // Set environment variables that will survive BenchmarkDotNet process isolation
        Environment.SetEnvironmentVariable("BENCHMARK_DATASET_SIZE", datasetSize.ToString());
        Environment.SetEnvironmentVariable("BENCHMARK_QUERY_COUNT", queries.ToString());
        Environment.SetEnvironmentVariable("BENCHMARK_CHARACTERISTIC", characteristic.ToString());

        // Select BenchmarkDotNet configuration
        IConfig config = accuracy switch
        {
            AccuracyLevel.Accurate => new AccurateConfig(),
            AccuracyLevel.Quick => new QuickConfig(),
            _ => new BalancedConfig()
        };


        // Execute the benchmark
        Console.WriteLine($"🏃 Running {test.ToDisplayString()} benchmark...");
        var summary = test switch
        {
            TestType.Construction => BenchmarkRunner.Run<ConstructionBenchmarks>(config),
            TestType.RangeQuery => BenchmarkRunner.Run<RangeQueryBenchmarks>(config),
            TestType.PointQuery => BenchmarkRunner.Run<PointQueryBenchmarks>(config),
            TestType.Allocation => BenchmarkRunner.Run<QueryAllocationBenchmarks>(config),
            _ => throw new InvalidOperationException($"Unknown test type: {test}")
        };
    }

    private static List<DatasetSize> ParseSizes(string sizes)
    {
        var result = new List<DatasetSize>();
        foreach (var size in sizes.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Enum.TryParse<DatasetSize>(size.Trim(), out var parsed))
            {
                result.Add(parsed);
            }
            else
            {
                Console.WriteLine($"⚠️  Warning: Unknown size '{size}', skipping");
            }
        }
        return result;
    }

    private static List<TestType> ParseTests(string tests)
    {
        var result = new List<TestType>();
        foreach (var test in tests.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Enum.TryParse<TestType>(test.Trim(), out var parsed))
            {
                result.Add(parsed);
            }
            else
            {
                Console.WriteLine($"⚠️  Warning: Unknown test '{test}', skipping");
            }
        }
        return result;
    }

    private static List<DatasetCharacteristic> ParseCharacteristics(string characteristics)
    {
        var result = new List<DatasetCharacteristic>();
        foreach (var characteristic in characteristics.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Enum.TryParse<DatasetCharacteristic>(characteristic.Trim(), out var parsed))
            {
                result.Add(parsed);
            }
            else
            {
                Console.WriteLine($"⚠️  Warning: Unknown characteristic '{characteristic}', skipping");
            }
        }
        return result;
    }

    private static void OrganizeResults(string outputDir, string timestampStr)
    {
        var artifactsDir = Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "results");
        var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), outputDir);
        
        Directory.CreateDirectory(resultsDir);
        
        if (!Directory.Exists(artifactsDir))
        {
            Console.WriteLine($"⚠️  Warning: Artifacts directory not found at {artifactsDir}");
            return;
        }

        var prefix = !string.IsNullOrEmpty(timestampStr) ? $"{timestampStr}_" : "";
        var filesCopied = 0;
        
        foreach (var file in Directory.GetFiles(artifactsDir, "*.csv").Concat(Directory.GetFiles(artifactsDir, "*.md")))
        {
            var fileName = Path.GetFileName(file);
            var fileTimestamp = File.GetLastWriteTime(file).ToString("HHmmss");
            var newFileName = $"{prefix}{fileTimestamp}_{fileName}";
            var destPath = Path.Combine(resultsDir, newFileName);
            
            File.Copy(file, destPath, true);
            filesCopied++;
        }
        
        Console.WriteLine($"📁 Copied {filesCopied} result files to: {resultsDir}");
    }

    private static void CleanArtifactsDirectory()
    {
        var artifactsDir = Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "results");
        
        if (Directory.Exists(artifactsDir))
        {
            foreach (var file in Directory.GetFiles(artifactsDir, "*.csv").Concat(Directory.GetFiles(artifactsDir, "*.md")))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Warning: Could not delete {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }
    }

    private static void OrganizeResultsBySize(string outputDir, string timestampStr, TestType test, DatasetSize size, DatasetCharacteristic characteristic)
    {
        var artifactsDir = Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "results");
        var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), outputDir);
        
        // Create timestamp-based directory structure: results/{yymmddhhmmss}/{size}/{characteristic}/
        var timestampDirName = !string.IsNullOrEmpty(timestampStr) ? timestampStr : DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var timestampDir = Path.Combine(resultsDir, timestampDirName);
        var sizeDir = Path.Combine(timestampDir, size.ToDisplayString());
        var characteristicDir = Path.Combine(sizeDir, characteristic.ToDisplayString());
        Directory.CreateDirectory(characteristicDir);
        
        if (!Directory.Exists(artifactsDir))
        {
            return;
        }

        var benchmarkTimestamp = DateTime.Now.ToString("HHmmss");
        
        // Only copy files that match the current test type (now should be clean artifacts)
        var testClassPattern = GetBenchmarkClassPattern(test);
        var filesCopied = 0;
        
        foreach (var file in Directory.GetFiles(artifactsDir, "*.csv").Concat(Directory.GetFiles(artifactsDir, "*.md")))
        {
            var fileName = Path.GetFileName(file);
            
            // Only copy files that exactly match the test class pattern
            if (fileName.Contains(testClassPattern))
            {
                var newFileName = $"{test.ToDisplayString()}_{characteristic.ToDisplayString()}_{benchmarkTimestamp}_{fileName}";
                var destPath = Path.Combine(characteristicDir, newFileName);
                
                File.Copy(file, destPath, true);
                filesCopied++;
            }
        }
        
        Console.WriteLine($"📁 Organized {filesCopied} {test.ToDisplayString()} result files in: {timestampDirName}/{size.ToDisplayString()}/{characteristic.ToDisplayString()}/");
    }
    
    private static string GetBenchmarkClassPattern(TestType test)
    {
        return test switch
        {
            TestType.Construction => "ConstructionBenchmarks",
            TestType.RangeQuery => "RangeQueryBenchmarks", 
            TestType.PointQuery => "PointQueryBenchmarks",
            TestType.Allocation => "QueryAllocationBenchmarks",
            _ => ""
        };
    }

    private static void GenerateSuiteSummary(
        string outputDir, 
        string timestampStr, 
        AccuracyLevel accuracy, 
        List<(TestType test, DatasetSize size, DatasetCharacteristic characteristic, bool success, string error)> results,
        TimeSpan totalTime)
    {
        var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), outputDir);
        
        // Create timestamp-based directory structure: results/{yymmddhhmmss}/
        var timestampDirName = !string.IsNullOrEmpty(timestampStr) ? timestampStr : DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var timestampDir = Path.Combine(resultsDir, timestampDirName);
        Directory.CreateDirectory(timestampDir);
        
        var summaryFile = Path.Combine(timestampDir, "SUITE_SUMMARY.txt");
        
        var successful = results.Count(r => r.success);
        var failed = results.Count(r => !r.success);
        
        var content = $@"RangeFinder Benchmark Suite Summary
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Working Directory: {Directory.GetCurrentDirectory()}
Total Execution Time: {totalTime:hh\:mm\:ss}

Configuration:
- Accuracy Level: {accuracy.ToDisplayString()}
- Total Configurations: {results.Count}
- Successful: {successful}
- Failed: {failed}
- Execution: Sequential (required for accurate benchmarking)

Benchmark Results:
{string.Join(Environment.NewLine, results.Select(r => $"- {r.test.ToDisplayString()} ({r.size.ToDisplayString()}) [{r.characteristic.ToDisplayString()}]: {(r.success ? "✅ SUCCESS" : $"❌ FAILED - {r.error}")}"))}

Platform Information:
- OS: {Environment.OSVersion}
- .NET Version: {Environment.Version}
- Processor Count: {Environment.ProcessorCount}
- Working Set: {Environment.WorkingSet / 1024 / 1024} MB

Generated Files:
{string.Join(Environment.NewLine, GetAllGeneratedFiles(timestampDir).Select(f => $"- {Path.GetRelativePath(timestampDir, f)}"))}

Notes:
- All benchmarks run in Release mode with sequential execution
- Results include both CSV data and markdown reports  
- Memory measurements may require additional investigation (see issue #14)
- For detailed analysis, see SUMMARY.md and individual result files
";
        
        File.WriteAllText(summaryFile, content);
        Console.WriteLine($"📝 Suite summary generated: {Path.GetFileName(summaryFile)}");
    }

    private static IEnumerable<string> GetAllGeneratedFiles(string timestampDir)
    {
        if (!Directory.Exists(timestampDir))
            return Enumerable.Empty<string>();
            
        var files = new List<string>();
        
        // Get all CSV and MD files in subdirectories
        foreach (var subDir in Directory.GetDirectories(timestampDir))
        {
            files.AddRange(Directory.GetFiles(subDir, "*.csv"));
            files.AddRange(Directory.GetFiles(subDir, "*.md"));
        }
        
        // Get any files directly in the timestamp directory (like SUITE_SUMMARY.txt)
        files.AddRange(Directory.GetFiles(timestampDir, "*.txt"));
        files.AddRange(Directory.GetFiles(timestampDir, "*.csv"));
        files.AddRange(Directory.GetFiles(timestampDir, "*.md"));
        
        return files.OrderBy(f => f);
    }
}