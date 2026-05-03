using DoubleCheck.Models;

namespace DoubleCheck.Services;

public interface IDuplicateDetectionService
{
    List<DuplicatePair> Detect(IEnumerable<ContactRecord> contacts);
    List<DuplicatePair> Detect(IEnumerable<AccountRecord> accounts);
}
