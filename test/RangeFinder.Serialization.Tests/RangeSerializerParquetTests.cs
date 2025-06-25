using RangeFinder.Core;
using RangeFinder.IO;
using RangeFinder.IO.Serialization;

namespace RangeFinder.IO.Tests;

[TestFixture]
public class RangeSerializerParquetTests
{
    private string GetTempFilePath() => Path.GetTempFileName().Replace(".tmp", ".parquet");

    [Test]
    public void ParquetSaveAndLoad_IntegerRanges_PreservesData()
    {
        var tempFilePath = GetTempFilePath();
        try
        {
            var originalRanges = new[]
            {
                new NumericRange<int, string>(1, 10, "Range1"),
                new NumericRange<int, string>(20, 30, "Range2"),
                new NumericRange<int, string>(40, 50, "Range3")
            };

            originalRanges.WriteParquet(tempFilePath);
            var loadedRanges = RangeSerializer.ReadParquet<int, string>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
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
    public void ParquetSaveAndLoad_DoubleRanges_PreservesData()
    {
        var tempFilePath = GetTempFilePath();
        try
        {
            var originalRanges = new[]
            {
                new NumericRange<double, int>(1.5, 10.7, 100),
                new NumericRange<double, int>(20.1, 30.9, 200),
                new NumericRange<double, int>(40.2, 50.8, 300)
            };

            originalRanges.WriteParquet(tempFilePath);
            var loadedRanges = RangeSerializer.ReadParquet<double, int>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
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
    public void ParquetSaveAndLoad_StringWithCommas_HandlesEscaping()
    {
        var tempFilePath = GetTempFilePath();
        try
        {
            var originalRanges = new[]
            {
                new NumericRange<int, string>(1, 10, "Range with, comma"),
                new NumericRange<int, string>(20, 30, "Another, value, with, commas"),
                new NumericRange<int, string>(40, 50, "Normal value")
            };

            originalRanges.WriteParquet(tempFilePath);
            var loadedRanges = RangeSerializer.ReadParquet<int, string>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
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
    public async Task ParquetSaveAndLoadAsync_IntegerRanges_PreservesData()
    {
        var tempFilePath = GetTempFilePath();
        try
        {
            var originalRanges = new[]
            {
                new NumericRange<int, string>(1, 10, "Range1"),
                new NumericRange<int, string>(20, 30, "Range2"),
                new NumericRange<int, string>(40, 50, "Range3")
            };

            await originalRanges.WriteParquetAsync(tempFilePath);
            var loadedRanges = (await RangeSerializer.ReadParquetAsync<int, string>(tempFilePath)).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
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
    public void ParquetSaveAndLoad_EmptyCollection_CreatesEmptyFile()
    {
        var tempFilePath = GetTempFilePath();
        try
        {
            var originalRanges = Array.Empty<NumericRange<int, string>>();

            originalRanges.WriteParquet(tempFilePath);
            var loadedRanges = RangeSerializer.ReadParquet<int, string>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.Empty);
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
    public void ParquetSaveAndLoad_NullValues_HandlesCorrectly()
    {
        var tempFilePath = GetTempFilePath();
        try
        {
            var originalRanges = new[]
            {
                new NumericRange<int, string?>(1, 10, null),
                new NumericRange<int, string?>(20, 30, "Range2"),
                new NumericRange<int, string?>(40, 50, null)
            };

            originalRanges.WriteParquet(tempFilePath);
            var loadedRanges = RangeSerializer.ReadParquet<int, string?>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
