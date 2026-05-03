using System.Text.Json;
using System.Text.Json.Serialization;
using DoubleCheck.Models;

namespace DoubleCheck.Services;

public sealed class LocalDataService : IDataverseService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataDirectory;

    public LocalDataService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
    }

    public Task<List<ContactRecord>> GetContactsAsync(CancellationToken ct = default)
    {
        var path    = Path.Combine(_dataDirectory, "contacts.json");
        var entries = LoadJson<JsonContact>(path);
        var records = entries
            .Select(e => new ContactRecord(Guid.Parse(e.Id), e.FullName, e.Email))
            .ToList();
        return Task.FromResult(records);
    }

    public Task<List<AccountRecord>> GetAccountsAsync(CancellationToken ct = default)
    {
        var path    = Path.Combine(_dataDirectory, "accounts.json");
        var entries = LoadJson<JsonAccount>(path);
        var records = entries
            .Select(e => new AccountRecord(Guid.Parse(e.Id), e.Name, e.Email))
            .ToList();
        return Task.FromResult(records);
    }

    private List<T> LoadJson<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Local data file not found: {path}");

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions)
            ?? throw new InvalidDataException($"Failed to deserialize {path}");
    }

    private record JsonContact(
        string Id,
        string FullName,
        [property: JsonPropertyName("email")] string? Email);

    private record JsonAccount(
        string Id,
        string Name,
        [property: JsonPropertyName("email")] string? Email);
}
