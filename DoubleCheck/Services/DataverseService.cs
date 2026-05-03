using DoubleCheck.Configuration;
using DoubleCheck.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace DoubleCheck.Services;

public sealed class DataverseService : IDataverseService, IDisposable
{
    private readonly ServiceClient _client;

    public DataverseService(AppSettings settings)
    {
        var connectionString =
            $"AuthType=ClientSecret;" +
            $"Url={settings.DataverseUrl};" +
            $"ClientId={settings.ClientId};" +
            $"ClientSecret={settings.ClientSecret};" +
            $"TenantId={settings.TenantId}";

        _client = new ServiceClient(connectionString);

        if (!_client.IsReady)
            throw new InvalidOperationException(
                $"Dataverse connection failed: {_client.LastError}");
    }

    public async Task<List<ContactRecord>> GetContactsAsync(CancellationToken ct = default)
    {
        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("contactid", "fullname", "emailaddress1"),
            PageInfo  = new PagingInfo { Count = 5000, PageNumber = 1 }
        };

        return await FetchAllPagesAsync(query, ct, entity =>
        {
            var id    = entity.GetAttributeValue<Guid>("contactid");
            var name  = entity.GetAttributeValue<string>("fullname") ?? string.Empty;
            var email = entity.GetAttributeValue<string>("emailaddress1");
            return new ContactRecord(id, name, email);
        });
    }

    public async Task<List<AccountRecord>> GetAccountsAsync(CancellationToken ct = default)
    {
        var query = new QueryExpression("account")
        {
            ColumnSet = new ColumnSet("accountid", "name", "emailaddress1"),
            PageInfo  = new PagingInfo { Count = 5000, PageNumber = 1 }
        };

        return await FetchAllPagesAsync(query, ct, entity =>
        {
            var id    = entity.GetAttributeValue<Guid>("accountid");
            var name  = entity.GetAttributeValue<string>("name") ?? string.Empty;
            var email = entity.GetAttributeValue<string>("emailaddress1");
            return new AccountRecord(id, name, email);
        });
    }

    private async Task<List<T>> FetchAllPagesAsync<T>(
        QueryExpression query,
        CancellationToken ct,
        Func<Microsoft.Xrm.Sdk.Entity, T> mapper)
    {
        var results = new List<T>();

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var response = await Task.Run(
                () => _client.RetrieveMultiple(query), ct);

            results.AddRange(response.Entities.Select(mapper));

            if (!response.MoreRecords) break;

            query.PageInfo.PageNumber++;
            query.PageInfo.PagingCookie = response.PagingCookie;
        }

        return results;
    }

    public void Dispose() => _client.Dispose();
}
