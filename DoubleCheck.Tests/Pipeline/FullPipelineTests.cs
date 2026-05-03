using DoubleCheck.Configuration;
using DoubleCheck.Reporting;
using DoubleCheck.Services;
using FluentAssertions;
using Xunit;

namespace DoubleCheck.Tests.Pipeline;

/// <summary>
/// Tests d'intégration end-to-end : données locales → détection → rapport CSV.
/// </summary>
public class FullPipelineTests : IDisposable
{
    private static readonly string DataDir =
        Path.Combine(AppContext.BaseDirectory, "data");

    private readonly string _outputDir =
        Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    private string OutputPath => Path.Combine(_outputDir, "pipeline_output.csv");

    [Fact]
    public async Task Pipeline_LocalData_DetectsAndWritesCsv()
    {
        var settings  = new AppSettings { SimilarityThreshold = 80, OutputCsvPath = OutputPath };
        var dataverse = new LocalDataService(DataDir);
        var detector  = new DuplicateDetectionService(settings);
        var reporter  = new CsvReportWriter();

        var contacts = await dataverse.GetContactsAsync();
        var accounts = await dataverse.GetAccountsAsync();

        var pairs = detector.Detect(contacts)
            .Concat(detector.Detect(accounts))
            .ToList();

        await reporter.WriteAsync(pairs, OutputPath);

        // Le fichier doit exister et contenir des données
        File.Exists(OutputPath).Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(OutputPath);
        lines.Length.Should().BeGreaterThan(1); // au moins header + 1 paire
    }

    [Fact]
    public async Task Pipeline_LocalData_FindsExpectedDuplicates()
    {
        var settings  = new AppSettings { SimilarityThreshold = 80 };
        var dataverse = new LocalDataService(DataDir);
        var detector  = new DuplicateDetectionService(settings);

        var contacts = await dataverse.GetContactsAsync();
        var accounts = await dataverse.GetAccountsAsync();

        var contactPairs = detector.Detect(contacts);
        var accountPairs = detector.Detect(accounts);

        // Alice Dupont / Alice Dupond → SimilarName
        contactPairs.Should().Contain(p =>
            p.Record1Name == "Alice Dupont" && p.Record2Name == "Alice Dupond" ||
            p.Record1Name == "Alice Dupond" && p.Record2Name == "Alice Dupont");

        // Acme Corp / Acme Corps → SimilarName
        accountPairs.Should().Contain(p =>
            (p.Record1Name == "Acme Corp" || p.Record2Name == "Acme Corp") &&
            (p.Record1Name == "Acme Corps" || p.Record2Name == "Acme Corps"));
    }

    [Fact]
    public async Task Pipeline_NullEmailRecords_DoNotCrash()
    {
        var settings  = new AppSettings { SimilarityThreshold = 85 };
        var dataverse = new LocalDataService(DataDir);
        var detector  = new DuplicateDetectionService(settings);

        var contacts = await dataverse.GetContactsAsync();

        // Charlie Durand a email null — ne doit pas lever d'exception
        var act = () => detector.Detect(contacts);
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, recursive: true);
    }
}
