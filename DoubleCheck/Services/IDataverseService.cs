using DoubleCheck.Models;

namespace DoubleCheck.Services;

public interface IDataverseService
{
    Task<List<ContactRecord>> GetContactsAsync(CancellationToken ct = default);
    Task<List<AccountRecord>> GetAccountsAsync(CancellationToken ct = default);
}
