using DoubleCheck.Models;
using DoubleCheck.Reporting;
using FluentAssertions;

namespace DoubleCheck.Tests.Reporting;

public class CsvReportWriterTests : IDisposable
{
    private readonly string _outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private string OutputPath => Path.Combine(_outputDir, "test_output.csv");

    [Fact]
    public async Task WriteAsync_CreatesMissingDirectory()
    {
        await new CsvReportWriter().WriteAsync([], OutputPath);
        Directory.Exists(_outputDir).Should().BeTrue();
    }

    [Fact]
    public async Task WriteAsync_EmptyList_WritesHeaderOnly()
    {
        await new CsvReportWriter().WriteAsync([], OutputPath);
        var lines = await File.ReadAllLinesAsync(OutputPath);
        lines.Should().HaveCount(1);
        lines[0].Should().Contain("Entity Type");
        lines[0].Should().Contain("Match Reason");
        lines[0].Should().Contain("Similarity %");
    }

    [Fact]
    public async Task WriteAsync_WithPairs_WritesCorrectRowCount()
    {
        var pairs = new[]
        {
            new DuplicatePair
            {
                EntityType    = "Contact",
                Record1Id     = Guid.NewGuid(),
                Record1Name   = "Alice Dupont",
                Record2Id     = Guid.NewGuid(),
                Record2Name   = "Alice Dupond",
                MatchReason   = "SimilarName",
                SimilarityPct = 92
            },
            new DuplicatePair
            {
                EntityType    = "Account",
                Record1Id     = Guid.NewGuid(),
                Record1Name   = "Acme Corp",
                Record2Id     = Guid.NewGuid(),
                Record2Name   = "Acme Corporation",
                MatchReason   = "ExactEmail",
                MatchedValue  = "info@acme.com",
                SimilarityPct = 100
            }
        };

        await new CsvReportWriter().WriteAsync(pairs, OutputPath);

        var lines = await File.ReadAllLinesAsync(OutputPath);
        lines.Should().HaveCount(3); // 1 header + 2 rows
    }

    [Fact]
    public async Task WriteAsync_OutputIsUtf8()
    {
        var pairs = new[]
        {
            new DuplicatePair
            {
                EntityType    = "Contact",
                Record1Id     = Guid.NewGuid(),
                Record1Name   = "Éric Côté",
                Record2Id     = Guid.NewGuid(),
                Record2Name   = "Eric Cote",
                MatchReason   = "SimilarName",
                SimilarityPct = 88
            }
        };

        await new CsvReportWriter().WriteAsync(pairs, OutputPath);

        var content = await File.ReadAllTextAsync(OutputPath, System.Text.Encoding.UTF8);
        content.Should().Contain("Éric Côté");
    }

    [Fact]
    public async Task WriteAsync_ColumnNamesAreStable()
    {
        await new CsvReportWriter().WriteAsync([], OutputPath);
        var header = (await File.ReadAllLinesAsync(OutputPath))[0];

        header.Should().Contain("Entity Type");
        header.Should().Contain("Record 1 ID");
        header.Should().Contain("Record 1 Name");
        header.Should().Contain("Record 2 ID");
        header.Should().Contain("Record 2 Name");
        header.Should().Contain("Match Reason");
        header.Should().Contain("Matched Value");
        header.Should().Contain("Similarity %");
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, recursive: true);
    }
}
