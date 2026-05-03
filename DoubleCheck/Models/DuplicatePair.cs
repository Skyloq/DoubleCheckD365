namespace DoubleCheck.Models;

public class DuplicatePair
{
    public string EntityType    { get; init; } = string.Empty;
    public Guid   Record1Id     { get; init; }
    public string Record1Name   { get; init; } = string.Empty;
    public Guid   Record2Id     { get; init; }
    public string Record2Name   { get; init; } = string.Empty;

    /// <summary>"ExactEmail" or "SimilarName"</summary>
    public string MatchReason   { get; init; } = string.Empty;

    /// <summary>The shared email address when MatchReason is ExactEmail.</summary>
    public string? MatchedValue { get; init; }

    /// <summary>Name similarity score (0–100). 100 for exact email matches.</summary>
    public int SimilarityPct    { get; init; }
}
