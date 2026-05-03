using DoubleCheck.Algorithms;
using DoubleCheck.Configuration;
using DoubleCheck.Models;

namespace DoubleCheck.Services;

public sealed class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly int _threshold;

    public DuplicateDetectionService(AppSettings settings)
    {
        _threshold = settings.SimilarityThreshold;
    }

    public List<DuplicatePair> Detect(IEnumerable<ContactRecord> contacts) =>
        DetectCore(
            contacts.Select(c => new Entry(c.Id, c.FullName, c.Email)),
            "Contact");

    public List<DuplicatePair> Detect(IEnumerable<AccountRecord> accounts) =>
        DetectCore(
            accounts.Select(a => new Entry(a.Id, a.Name, a.Email)),
            "Account");

    private List<DuplicatePair> DetectCore(IEnumerable<Entry> source, string entityType)
    {
        var entries = source.ToList();
        var pairs   = new List<DuplicatePair>();
        var seen    = new HashSet<(Guid, Guid)>();

        // Pass 1 — exact email match
        var byEmail = new Dictionary<string, List<Entry>>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries.Where(e => !string.IsNullOrWhiteSpace(e.Email)))
        {
            var key = entry.Email!.Trim();
            if (!byEmail.TryGetValue(key, out var bucket))
                byEmail[key] = bucket = [];
            bucket.Add(entry);
        }

        foreach (var (email, bucket) in byEmail.Where(kv => kv.Value.Count > 1))
        {
            for (var i = 0; i < bucket.Count - 1; i++)
            for (var j = i + 1; j < bucket.Count; j++)
            {
                var pair = MakePair(entityType, bucket[i], bucket[j], "ExactEmail", email, 100);
                pairs.Add(pair);
                seen.Add(OrderedKey(bucket[i].Id, bucket[j].Id));
            }
        }

        // Pass 2 — similar names via Levenshtein
        for (var i = 0; i < entries.Count - 1; i++)
        for (var j = i + 1; j < entries.Count; j++)
        {
            var key = OrderedKey(entries[i].Id, entries[j].Id);
            if (seen.Contains(key)) continue;

            var sim = LevenshteinCalculator.SimilarityPercent(
                entries[i].Name.ToLowerInvariant(),
                entries[j].Name.ToLowerInvariant());

            if (sim < _threshold) continue;

            pairs.Add(MakePair(entityType, entries[i], entries[j], "SimilarName", null, sim));
            seen.Add(key);
        }

        return pairs;
    }

    private static DuplicatePair MakePair(
        string entityType, Entry a, Entry b,
        string reason, string? matchedValue, int similarityPct) =>
        new()
        {
            EntityType    = entityType,
            Record1Id     = a.Id,
            Record1Name   = a.Name,
            Record2Id     = b.Id,
            Record2Name   = b.Name,
            MatchReason   = reason,
            MatchedValue  = matchedValue,
            SimilarityPct = similarityPct
        };

    private static (Guid, Guid) OrderedKey(Guid a, Guid b) =>
        a.CompareTo(b) <= 0 ? (a, b) : (b, a);

    private record Entry(Guid Id, string Name, string? Email);
}
