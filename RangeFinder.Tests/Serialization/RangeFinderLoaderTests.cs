using RangeFinder.Core;
using RangeFinder.IO;
using RangeFinder.IO.Serialization;

namespace RangeFinder.Tests.Serialization;

/// <summary>
/// Tests for RangeFinderLoader class.
/// Validates CSV and Parquet file loading with various data types and error handling.
/// </summary>
[TestFixture]
public class RangeFinderLoaderTests
{
    private string GetTempCsvPath() => Path.GetTempFileName().Replace(".tmp", ".csv");
    private string GetTempParquetPath() => Path.GetTempFileName().Replace(".tmp", ".parquet");

    [TearDown]
    public void Cleanup()
    {
        // Clean up any temp files that might have been created
        var tempFiles = Directory.GetFiles(Path.GetTempPath(), "tmp*.csv")
            .Concat(Directory.GetFiles(Path.GetTempPath(), "tmp*.parquet"));

        foreach (var file in tempFiles)
        {
            try { File.Delete(file); } catch { /* ignore cleanup errors */ }
        }
    }

    #region CSV Tests

    [Test]
    public void FromCsv_DefaultTypes_CreatesValidRangeFinder()
    {
        var tempFilePath = GetTempCsvPath();
        try
        {
            // Create test data
            var testRanges = new[]
            {
                new NumericRange<double, string>(1.0, 2.0, "First"),
                new NumericRange<double, string>(3.0, 4.0, "Second"),
                new NumericRange<double, string>(2.5, 3.5, "Third")
            };

            // Save to CSV
            testRanges.WriteCsv(tempFilePath);

            // Load using loader
            var rangeFinder = RangeFinderLoader.FromCsv(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(3));
            Assert.That(rangeFinder.LowerBound, Is.EqualTo(1.0));
            Assert.That(rangeFinder.UpperBound, Is.EqualTo(4.0));

            var results = rangeFinder.QueryRanges(2.7).ToArray();
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].Value, Is.EqualTo("Third"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public void FromCsv_CustomTypes_CreatesValidRangeFinder()
    {
        var tempFilePath = GetTempCsvPath();
        try
        {
            // Create test data with int ranges and int values
            var testRanges = new[]
            {
                new NumericRange<int, int>(10, 20, 100),
                new NumericRange<int, int>(30, 40, 200),
                new NumericRange<int, int>(25, 35, 300)
            };

            // Save to CSV
            testRanges.WriteCsv(tempFilePath);

            // Load using loader with custom types
            var rangeFinder = RangeFinderLoader.FromCsv<int, int>(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(3));
            var results = rangeFinder.QueryRanges(32).ToArray();
            Assert.That(results, Has.Length.EqualTo(2));
            Assert.That(results.Select(r => r.Value), Contains.Item(200));
            Assert.That(results.Select(r => r.Value), Contains.Item(300));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public async Task FromCsvAsync_DefaultTypes_CreatesValidRangeFinder()
    {
        var tempFilePath = GetTempCsvPath();
        try
        {
            // Create test data
            var testRanges = new[]
            {
                new NumericRange<double, string>(1.0, 2.0, "Async1"),
                new NumericRange<double, string>(3.0, 4.0, "Async2")
            };

            // Save to CSV
            await testRanges.WriteCsvAsync(tempFilePath);

            // Load using async loader
            var rangeFinder = await RangeFinderLoader.FromCsvAsync(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(2));
            var results = rangeFinder.QueryRanges(1.5).ToArray();
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].Value, Is.EqualTo("Async1"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public async Task FromCsvAsync_CustomTypes_CreatesValidRangeFinder()
    {
        var tempFilePath = GetTempCsvPath();
        try
        {
            // Create test data with decimal ranges
            var testRanges = new[]
            {
                new NumericRange<decimal, int>(1.5m, 2.5m, 42),
                new NumericRange<decimal, int>(3.1m, 4.1m, 84)
            };

            // Save to CSV
            await testRanges.WriteCsvAsync(tempFilePath);

            // Load using async loader with custom types
            var rangeFinder = await RangeFinderLoader.FromCsvAsync<decimal, int>(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(2));
            var results = rangeFinder.QueryRanges(2.0m).ToArray();
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].Value, Is.EqualTo(42));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public void FromCsv_NullFilePath_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        string filePath = null;
        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderLoader.FromCsv(filePath));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("filePath"));
    }

    [Test]
    public void FromCsv_NonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "non_existent_file.csv");

        var ex = Assert.Throws<FileNotFoundException>(() => RangeFinderLoader.FromCsv(nonExistentPath));
        Assert.That(ex.Message, Contains.Substring(nonExistentPath));
    }

    [Test]
    public void FromCsvAsync_NullFilePath_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        string filePath = null;
        Assert.ThrowsAsync<ArgumentNullException>(() => RangeFinderLoader.FromCsvAsync(filePath));
#pragma warning restore CS8600, CS8604
    }

    #endregion

    #region Parquet Tests

    [Test]
    public void FromParquet_DefaultTypes_CreatesValidRangeFinder()
    {
        var tempFilePath = GetTempParquetPath();
        try
        {
            // Create test data
            var testRanges = new[]
            {
                new NumericRange<double, string>(5.0, 6.0, "ParquetFirst"),
                new NumericRange<double, string>(7.0, 8.0, "ParquetSecond"),
                new NumericRange<double, string>(6.5, 7.5, "ParquetThird")
            };

            // Save to Parquet
            testRanges.WriteParquet(tempFilePath);

            // Load using loader
            var rangeFinder = RangeFinderLoader.FromParquet(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(3));
            Assert.That(rangeFinder.LowerBound, Is.EqualTo(5.0));
            Assert.That(rangeFinder.UpperBound, Is.EqualTo(8.0));

            var results = rangeFinder.QueryRanges(6.8).ToArray();
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].Value, Is.EqualTo("ParquetThird"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public void FromParquet_CustomTypes_CreatesValidRangeFinder()
    {
        var tempFilePath = GetTempParquetPath();
        try
        {
            // Create test data with float ranges and int values
            var testRanges = new[]
            {
                new NumericRange<float, int>(1.1f, 2.1f, 111),
                new NumericRange<float, int>(3.1f, 4.1f, 222)
            };

            // Save to Parquet
            testRanges.WriteParquet(tempFilePath);

            // Load using loader with custom types
            var rangeFinder = RangeFinderLoader.FromParquet<float, int>(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(2));
            var results = rangeFinder.QueryRanges(1.6f).ToArray();
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].Value, Is.EqualTo(111));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public async Task FromParquetAsync_DefaultTypes_CreatesValidRangeFinder()
    {
        var tempFilePath = GetTempParquetPath();
        try
        {
            // Create test data
            var testRanges = new[]
            {
                new NumericRange<double, string>(11.0, 12.0, "AsyncParquet1"),
                new NumericRange<double, string>(13.0, 14.0, "AsyncParquet2")
            };

            // Save to Parquet
            await testRanges.WriteParquetAsync(tempFilePath);

            // Load using async loader
            var rangeFinder = await RangeFinderLoader.FromParquetAsync(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(2));
            var results = rangeFinder.QueryRanges(11.5).ToArray();
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].Value, Is.EqualTo("AsyncParquet1"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public void FromParquet_NullFilePath_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        string filePath = null;
        var ex = Assert.Throws<ArgumentNullException>(() => RangeFinderLoader.FromParquet(filePath));
#pragma warning restore CS8600, CS8604
        Assert.That(ex.ParamName, Is.EqualTo("filePath"));
    }

    [Test]
    public void FromParquet_NonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "non_existent_file.parquet");

        var ex = Assert.Throws<FileNotFoundException>(() => RangeFinderLoader.FromParquet(nonExistentPath));
        Assert.That(ex.Message, Contains.Substring(nonExistentPath));
    }

    [Test]
    public void FromParquetAsync_NullFilePath_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604 // Intentional null assignment and passing for testing null handling
        string filePath = null;
        Assert.ThrowsAsync<ArgumentNullException>(() => RangeFinderLoader.FromParquetAsync(filePath));
#pragma warning restore CS8600, CS8604

    }
    #endregion

    #region Empty File Tests

    [Test]
    public void FromCsv_EmptyFile_CreatesEmptyRangeFinder()
    {
        var tempFilePath = GetTempCsvPath();
        try
        {
            // Create empty CSV file with just headers
            var emptyRanges = Enumerable.Empty<NumericRange<double, string>>();
            emptyRanges.WriteCsv(tempFilePath);

            var rangeFinder = RangeFinderLoader.FromCsv(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(0));
            Assert.That(rangeFinder.QueryRanges(1.0), Is.Empty);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public void FromParquet_EmptyFile_CreatesEmptyRangeFinder()
    {
        var tempFilePath = GetTempParquetPath();
        try
        {
            // Create empty Parquet file
            var emptyRanges = Enumerable.Empty<NumericRange<double, string>>();
            emptyRanges.WriteParquet(tempFilePath);

            var rangeFinder = RangeFinderLoader.FromParquet(tempFilePath);

            Assert.That(rangeFinder.Count, Is.EqualTo(0));
            Assert.That(rangeFinder.QueryRanges(1.0), Is.Empty);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    #endregion
}
