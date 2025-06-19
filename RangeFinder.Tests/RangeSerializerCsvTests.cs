using RangeFinder.Core;
using RangeFinder.IO;
using RangeFinder.IO.Serialization;

namespace RangeFinder.Tests;

[TestFixture]
public class RangeSerializerCsvTests
{
    private string GetTempFilePath() => Path.GetTempFileName().Replace(".tmp", ".csv");

    [Test]
    public void CsvSaveAndLoad_IntegerRanges_PreservesData()
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

            originalRanges.WriteCsv(tempFilePath);
            var loadedRanges = RangeSerializer.ReadCsv<int, string>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Test]
    public void CsvSaveAndLoad_DoubleRanges_PreservesData()
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

            originalRanges.WriteCsv(tempFilePath);
            var loadedRanges = RangeSerializer.ReadCsv<double, int>(tempFilePath).ToList();

            // For floating point, we need tolerance comparison
            Assert.Multiple(() =>
            {
                Assert.That(loadedRanges, Has.Count.EqualTo(3));
                Assert.That(loadedRanges.Select(r => r.Start), Is.EqualTo(originalRanges.Select(r => r.Start)).Within(0.001));
                Assert.That(loadedRanges.Select(r => r.End), Is.EqualTo(originalRanges.Select(r => r.End)).Within(0.001));
                Assert.That(loadedRanges.Select(r => r.Value), Is.EqualTo(originalRanges.Select(r => r.Value)));
            });
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Test]
    public void CsvSaveAndLoad_StringWithCommas_HandlesEscaping()
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

            originalRanges.WriteCsv(tempFilePath);
            var loadedRanges = RangeSerializer.ReadCsv<int, string>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Test]
    public async Task CsvSaveAndLoadAsync_IntegerRanges_PreservesData()
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

            await originalRanges.WriteCsvAsync(tempFilePath);
            var loadedRanges = (await RangeSerializer.ReadCsvAsync<int, string>(tempFilePath)).ToList();

            Assert.That(loadedRanges, Is.EqualTo(originalRanges));
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Test]
    public void CsvSaveAndLoad_EmptyCollection_CreatesEmptyFile()
    {
        var tempFilePath = GetTempFilePath();
        try
        {
            var originalRanges = Array.Empty<NumericRange<int, string>>();

            originalRanges.WriteCsv(tempFilePath);
            var loadedRanges = RangeSerializer.ReadCsv<int, string>(tempFilePath).ToList();

            Assert.That(loadedRanges, Is.Empty);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Test]
    public void CsvSaveAndLoad_NullValues_HandlesCorrectly()
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

            originalRanges.WriteCsv(tempFilePath);
            var loadedRanges = RangeSerializer.ReadCsv<int, string?>(tempFilePath).ToList();

            // CsvHelper converts null to empty string
            var expectedRanges = new[]
            {
                new NumericRange<int, string?>(1, 10, string.Empty),
                new NumericRange<int, string?>(20, 30, "Range2"),
                new NumericRange<int, string?>(40, 50, string.Empty)
            };
            Assert.That(loadedRanges, Is.EqualTo(expectedRanges));
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}