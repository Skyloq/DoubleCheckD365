using DoubleCheck.Configuration;
using DoubleCheck.Infrastructure;
using DoubleCheck.Reporting;
using DoubleCheck.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = configuration.GetSection("DoubleCheck").Get<AppSettings>()
    ?? throw new InvalidOperationException("Missing 'DoubleCheck' configuration section.");

var provider = new ServiceCollection()
    .AddDoubleCheckServices(settings)
    .BuildServiceProvider();

var dataverse = provider.GetRequiredService<IDataverseService>();
var detector  = provider.GetRequiredService<IDuplicateDetectionService>();
var reporter  = provider.GetRequiredService<IReportWriter>();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

Console.WriteLine("DoubleCheck — D365 Duplicate Clean-Up Tool");
Console.WriteLine("==========================================");
Console.WriteLine();

Console.Write("Fetching contacts... ");
var contacts = await dataverse.GetContactsAsync(cts.Token);
Console.WriteLine($"{contacts.Count} records loaded.");

Console.Write("Fetching accounts... ");
var accounts = await dataverse.GetAccountsAsync(cts.Token);
Console.WriteLine($"{accounts.Count} records loaded.");

Console.WriteLine();
Console.Write("Detecting duplicates... ");
var contactPairs = detector.Detect(contacts);
var accountPairs = detector.Detect(accounts);
var allPairs     = contactPairs.Concat(accountPairs).ToList();
Console.WriteLine($"{allPairs.Count} potential duplicate pairs found.");

if (allPairs.Count == 0)
{
    Console.WriteLine("\nNo duplicates detected. Nothing to report.");
    return 0;
}

Console.WriteLine();
Console.Write($"Writing report to '{settings.OutputCsvPath}'... ");
await reporter.WriteAsync(allPairs, settings.OutputCsvPath, cts.Token);
Console.WriteLine("Done.");

Console.WriteLine();
Console.WriteLine("Review the CSV before merging any records.");
Console.WriteLine($"  Contacts flagged : {contactPairs.Count} pairs");
Console.WriteLine($"  Accounts flagged : {accountPairs.Count} pairs");
Console.WriteLine($"  Threshold used   : {settings.SimilarityThreshold}% similarity");

return 0;
