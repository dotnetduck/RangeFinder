using Parquet;
using Parquet.Serialization;
using RangeFinder.Core;
using System.Numerics;

namespace RangeFinder.IO.Serialization;

public static partial class RangeSerializer
{
    static public IEnumerable<NumericRange<TNumber, TAssociated>>
    ReadParquet<TNumber, TAssociated>(string filePath)
    where TNumber : INumber<TNumber>
    {
        using var fileStream = File.OpenRead(filePath);
        return ParquetSerializer.DeserializeAsync<NumericRange<TNumber, TAssociated>>(fileStream).Result;
    }

    static public async Task<IEnumerable<NumericRange<TNumber, TAssociated>>>
    ReadParquetAsync<TNumber, TAssociated>(string filePath)
        where TNumber : INumber<TNumber>
    {
        using var fileStream = File.OpenRead(filePath);
        return await ParquetSerializer.DeserializeAsync<NumericRange<TNumber, TAssociated>>(fileStream);
    }

    public static void WriteParquet<TNumber, TAssociated>(
        this IEnumerable<NumericRange<TNumber, TAssociated>> ranges,
        string filePath
        ) where TNumber : INumber<TNumber>
    {
        using var fileStream = File.Create(filePath);
        ParquetSerializer.SerializeAsync(ranges, fileStream).Wait();
    }

    public static async Task WriteParquetAsync<TNumber, TAssociated>(
        this IEnumerable<NumericRange<TNumber, TAssociated>> ranges,
        string filePath
        ) where TNumber : INumber<TNumber>
    {
        using var fileStream = File.Create(filePath);
        await ParquetSerializer.SerializeAsync(ranges, fileStream);
    }
}
