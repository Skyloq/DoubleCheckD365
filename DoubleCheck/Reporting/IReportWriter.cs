using DoubleCheck.Models;

namespace DoubleCheck.Reporting;

public interface IReportWriter
{
    Task WriteAsync(IEnumerable<DuplicatePair> pairs, string outputPath,
                    CancellationToken ct = default);
}
