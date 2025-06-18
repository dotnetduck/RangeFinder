using CsvHelper;
using RangeFinder.Core;
using System.Globalization;
using System.Numerics;

namespace RangeFinder.IO;

public static partial class RangeSerializer
{
    static public IEnumerable<NumericRange<TNumber, TAssociated>>
    ReadCsv<TNumber, TAssociated>(string filePath)
    where TNumber : INumber<TNumber>
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        foreach (var record in csv.GetRecords<NumericRange<TNumber, TAssociated>>())
        {
            yield return record;
        }
    }

    static public async Task<IEnumerable<NumericRange<TNumber, TAssociated>>>
    ReadCsvAsync<TNumber, TAssociated>(string filePath)
        where TNumber : INumber<TNumber>
    {
        var content = await File.ReadAllTextAsync(filePath);
        using var reader = new StringReader(content);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        return csv.GetRecords<NumericRange<TNumber, TAssociated>>().ToList();
    }

    public static void WriteCsv<TNumber, TAssociated>(
        this IEnumerable<NumericRange<TNumber, TAssociated>> ranges,
        string filePath
        ) where TNumber : INumber<TNumber>
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        csv.WriteRecords(ranges);
    }

    public static async Task WriteCsvAsync<TNumber, TAssociated>(
        this IEnumerable<NumericRange<TNumber, TAssociated>> ranges,
        string filePath
        ) where TNumber : INumber<TNumber>
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        await csv.WriteRecordsAsync(ranges);
    }
}
