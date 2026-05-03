using DoubleCheck.Configuration;
using DoubleCheck.Models;
using DoubleCheck.Services;
using FluentAssertions;

namespace DoubleCheck.Tests.Services;

public class DuplicateDetectionServiceTests
{
    private static DuplicateDetectionService BuildService(int threshold = 85) =>
        new(new AppSettings { SimilarityThreshold = threshold });

    // --- Cas limites ---

    [Fact]
    public void Detect_EmptyList_ReturnsNoPairs()
    {
        BuildService().Detect(Array.Empty<ContactRecord>()).Should().BeEmpty();
    }

    [Fact]
    public void Detect_SingleRecord_ReturnsNoPairs()
    {
        var contacts = new[] { new ContactRecord(Guid.NewGuid(), "Marie Tremblay", "m@test.com") };
        BuildService().Detect(contacts).Should().BeEmpty();
    }

    // --- Détection par email exact ---

    [Fact]
    public void Detect_SameEmail_FlagsAsExactEmail()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var contacts = new[]
        {
            new ContactRecord(id1, "Patrick Gagnon", "pgagnon@test.com"),
            new ContactRecord(id2, "Patrice Gagnon", "pgagnon@test.com")
        };

        var pairs = BuildService().Detect(contacts);

        pairs.Should().HaveCount(1);
        pairs[0].MatchReason.Should().Be("ExactEmail");
        pairs[0].MatchedValue.Should().Be("pgagnon@test.com");
        pairs[0].SimilarityPct.Should().Be(100);
    }

    [Fact]
    public void Detect_EmailComparison_IsCaseInsensitive()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "Alice Dupont", "Alice@Test.COM"),
            new ContactRecord(Guid.NewGuid(), "Alicia Dupont", "alice@test.com")
        };

        BuildService().Detect(contacts).Should().ContainSingle(p => p.MatchReason == "ExactEmail");
    }

    [Fact]
    public void Detect_NullEmail_NotFlaggedForEmailMatch()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "Marie Tremblay", null),
            new ContactRecord(Guid.NewGuid(), "Marie Tremblay", null)
        };

        var pairs = BuildService().Detect(contacts);
        pairs.Should().NotContain(p => p.MatchReason == "ExactEmail");
    }

    [Fact]
    public void Detect_ThreeRecordsSameEmail_ReportsAllPairs()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "A", "shared@test.com"),
            new ContactRecord(Guid.NewGuid(), "B", "shared@test.com"),
            new ContactRecord(Guid.NewGuid(), "C", "shared@test.com")
        };

        // 3 enregistrements → 3 paires (A-B, A-C, B-C)
        BuildService().Detect(contacts)
            .Where(p => p.MatchReason == "ExactEmail")
            .Should().HaveCount(3);
    }

    // --- Détection par similarité de nom ---

    [Fact]
    public void Detect_IdenticalNames_FlagsAsSimilarName()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "Sophie Lavoie", "s1@test.com"),
            new ContactRecord(Guid.NewGuid(), "Sophie Lavoie", "s2@test.com")
        };

        var pairs = BuildService().Detect(contacts);
        pairs.Should().ContainSingle(p => p.MatchReason == "SimilarName");
    }

    [Fact]
    public void Detect_SimilarNamesAboveThreshold_Flagged()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "Nathalie Bergeron", "n1@test.com"),
            new ContactRecord(Guid.NewGuid(), "Natalie Bergeron",  "n2@test.com")
        };

        BuildService(threshold: 85).Detect(contacts)
            .Should().ContainSingle(p => p.MatchReason == "SimilarName");
    }

    [Fact]
    public void Detect_SimilarNamesBelowThreshold_NotFlagged()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "Jean Tremblay", "j1@test.com"),
            new ContactRecord(Guid.NewGuid(), "Marc Gagnon",   "j2@test.com")
        };

        BuildService(threshold: 85).Detect(contacts)
            .Should().BeEmpty();
    }

    // --- Déduplication des paires ---

    [Fact]
    public void Detect_SameEmailAndSimilarName_ReportedOnlyOnce()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var contacts = new[]
        {
            new ContactRecord(id1, "Éric Lefebvre", "eric@test.com"),
            new ContactRecord(id2, "Eric Lefebvre",  "eric@test.com")
        };

        // Email exact prend la priorité, la paire ne doit pas apparaître deux fois
        BuildService().Detect(contacts).Should().HaveCount(1);
    }

    [Fact]
    public void Detect_PairNotReportedInBothDirections()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var contacts = new[]
        {
            new ContactRecord(id1, "Luc Beauchamp",  "l1@test.com"),
            new ContactRecord(id2, "Luc Beauchamps", "l2@test.com")
        };

        BuildService().Detect(contacts).Should().HaveCount(1);
    }

    // --- EntityType ---

    [Fact]
    public void Detect_Contacts_EntityTypeIsContact()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "Marie Tremblay", "a@test.com"),
            new ContactRecord(Guid.NewGuid(), "Marie Tremblay", "b@test.com")
        };

        BuildService().Detect(contacts)
            .Should().OnlyContain(p => p.EntityType == "Contact");
    }

    [Fact]
    public void Detect_Accounts_EntityTypeIsAccount()
    {
        var accounts = new[]
        {
            new AccountRecord(Guid.NewGuid(), "Acme Corporation", "a@test.com"),
            new AccountRecord(Guid.NewGuid(), "Acme Corp",        "b@test.com")
        };

        BuildService().Detect(accounts)
            .Should().OnlyContain(p => p.EntityType == "Account");
    }

    // --- Threshold configurable ---

    [Fact]
    public void Detect_LowerThreshold_FindsMorePairs()
    {
        var contacts = new[]
        {
            new ContactRecord(Guid.NewGuid(), "Philippe Mercier", "p1@test.com"),
            new ContactRecord(Guid.NewGuid(), "Philip Mercier",   "p2@test.com")
        };

        var strictPairs = BuildService(threshold: 97).Detect(contacts);
        var loosePairs  = BuildService(threshold: 80).Detect(contacts);

        loosePairs.Count.Should().BeGreaterThanOrEqualTo(strictPairs.Count);
    }
}
