using DoubleCheck.Services;
using FluentAssertions;

namespace DoubleCheck.Tests.Services;

public class LocalDataServiceTests
{
    private static readonly string DataDir =
        Path.Combine(AppContext.BaseDirectory, "data");

    private LocalDataService BuildService() => new(DataDir);

    // --- Contacts ---

    [Fact]
    public async Task GetContactsAsync_LoadsAllRecords()
    {
        var contacts = await BuildService().GetContactsAsync();
        contacts.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetContactsAsync_MapsFieldsCorrectly()
    {
        var contacts = await BuildService().GetContactsAsync();
        var alice = contacts.Should().ContainSingle(c => c.FullName == "Alice Dupont").Subject;

        alice.Id.Should().Be(Guid.Parse("c3000000-0000-0000-0000-000000000001"));
        alice.Email.Should().Be("alice@test.com");
    }

    [Fact]
    public async Task GetContactsAsync_NullEmailHandledGracefully()
    {
        var contacts = await BuildService().GetContactsAsync();
        contacts.Should().ContainSingle(c => c.FullName == "Charlie Durand" && c.Email == null);
    }

    // --- Accounts ---

    [Fact]
    public async Task GetAccountsAsync_LoadsAllRecords()
    {
        var accounts = await BuildService().GetAccountsAsync();
        accounts.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAccountsAsync_MapsFieldsCorrectly()
    {
        var accounts = await BuildService().GetAccountsAsync();
        var acme = accounts.Should().ContainSingle(a => a.Name == "Acme Corp").Subject;

        acme.Id.Should().Be(Guid.Parse("d4000000-0000-0000-0000-000000000001"));
        acme.Email.Should().Be("info@acme.com");
    }

    // --- Erreurs ---

    [Fact]
    public async Task GetContactsAsync_MissingFile_ThrowsFileNotFoundException()
    {
        var service = new LocalDataService("/chemin/inexistant");
        await service.Invoking(s => s.GetContactsAsync())
            .Should().ThrowAsync<FileNotFoundException>();
    }
}
