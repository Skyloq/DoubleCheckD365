using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DoubleCheck.Models;

namespace DoubleCheck.Reporting;

public sealed class CsvReportWriter : IReportWriter
{
    public async Task WriteAsync(
        IEnumerable<DuplicatePair> pairs,
        string outputPath,
        CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var writer = new StreamWriter(outputPath, append: false, System.Text.Encoding.UTF8);
        await using var csv    = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.Context.RegisterClassMap<DuplicatePairMap>();
        await csv.WriteRecordsAsync(pairs, ct);
    }
}

internal sealed class DuplicatePairMap : ClassMap<DuplicatePair>
{
    public DuplicatePairMap()
    {
        Map(p => p.EntityType).Name("Entity Type");
        Map(p => p.Record1Id).Name("Record 1 ID");
        Map(p => p.Record1Name).Name("Record 1 Name");
        Map(p => p.Record2Id).Name("Record 2 ID");
        Map(p => p.Record2Name).Name("Record 2 Name");
        Map(p => p.MatchReason).Name("Match Reason");
        Map(p => p.MatchedValue).Name("Matched Value");
        Map(p => p.SimilarityPct).Name("Similarity %");
    }
}
