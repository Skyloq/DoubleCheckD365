namespace DoubleCheck.Configuration;

public class AppSettings
{
    public string DataverseUrl        { get; set; } = string.Empty;
    public string ClientId            { get; set; } = string.Empty;
    public string ClientSecret        { get; set; } = string.Empty;
    public string TenantId            { get; set; } = string.Empty;

    /// <summary>Minimum name similarity (0–100) to flag a pair as duplicate.</summary>
    public int SimilarityThreshold { get; set; } = 85;

    public string OutputCsvPath { get; set; } = "output/duplicates.csv";

    public bool   UseLocalData   { get; set; } = false;
    public string LocalDataPath  { get; set; } = "data";
}
