using RangeFinder.Core;
using System.Numerics;

namespace RangeFinder.Serialization;

/// <summary>
/// Loader for creating RangeFinder instances from CSV and Parquet files.
/// Provides convenient methods to load range data from files and create optimized RangeFinder instances.
/// </summary>
public static class RangeFinderLoader
{
    /// <summary>
    /// Creates a RangeFinder instance from a CSV file using default types (double, string).
    /// </summary>
    /// <param name="filePath">Path to the CSV file containing range data</param>
    /// <returns>A new RangeFinder instance with double ranges and string values</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static RangeFinder<double, string> FromCsv(string filePath)
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = RangeSerializer.ReadCsv<double, string>(filePath);
        return RangeFinderFactory.Create(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder instance from a CSV file with specified types.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <param name="filePath">Path to the CSV file containing range data</param>
    /// <returns>A new RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static RangeFinder<TNumber, TAssociated> FromCsv<TNumber, TAssociated>(string filePath)
        where TNumber : INumber<TNumber>
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = RangeSerializer.ReadCsv<TNumber, TAssociated>(filePath);
        return RangeFinderFactory.Create(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder instance from a CSV file asynchronously using default types (double, string).
    /// </summary>
    /// <param name="filePath">Path to the CSV file containing range data</param>
    /// <returns>A task that represents the asynchronous operation with a RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static async Task<RangeFinder<double, string>> FromCsvAsync(string filePath)
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = await RangeSerializer.ReadCsvAsync<double, string>(filePath);
        return RangeFinderFactory.Create(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder instance from a CSV file asynchronously with specified types.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <param name="filePath">Path to the CSV file containing range data</param>
    /// <returns>A task that represents the asynchronous operation with a RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static async Task<RangeFinder<TNumber, TAssociated>> FromCsvAsync<TNumber, TAssociated>(string filePath)
        where TNumber : INumber<TNumber>
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = await RangeSerializer.ReadCsvAsync<TNumber, TAssociated>(filePath);
        return RangeFinderFactory.Create(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder instance from a Parquet file using default types (double, string).
    /// </summary>
    /// <param name="filePath">Path to the Parquet file containing range data</param>
    /// <returns>A new RangeFinder instance with double ranges and string values</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static RangeFinder<double, string> FromParquet(string filePath)
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = RangeSerializer.ReadParquet<double, string>(filePath);
        return RangeFinderFactory.Create(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder instance from a Parquet file with specified types.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <param name="filePath">Path to the Parquet file containing range data</param>
    /// <returns>A new RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static RangeFinder<TNumber, TAssociated> FromParquet<TNumber, TAssociated>(string filePath)
        where TNumber : INumber<TNumber>
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = RangeSerializer.ReadParquet<TNumber, TAssociated>(filePath);
        return RangeFinderFactory.Create(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder instance from a Parquet file asynchronously using default types (double, string).
    /// </summary>
    /// <param name="filePath">Path to the Parquet file containing range data</param>
    /// <returns>A task that represents the asynchronous operation with a RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static async Task<RangeFinder<double, string>> FromParquetAsync(string filePath)
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = await RangeSerializer.ReadParquetAsync<double, string>(filePath);
        return RangeFinderFactory.Create(ranges);
    }

    /// <summary>
    /// Creates a RangeFinder instance from a Parquet file asynchronously with specified types.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type for range boundaries</typeparam>
    /// <typeparam name="TAssociated">The type of value associated with each range</typeparam>
    /// <param name="filePath">Path to the Parquet file containing range data</param>
    /// <returns>A task that represents the asynchronous operation with a RangeFinder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static async Task<RangeFinder<TNumber, TAssociated>> FromParquetAsync<TNumber, TAssociated>(string filePath)
        where TNumber : INumber<TNumber>
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var ranges = await RangeSerializer.ReadParquetAsync<TNumber, TAssociated>(filePath);
        return RangeFinderFactory.Create(ranges);
    }
}
