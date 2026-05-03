namespace DoubleCheck.Algorithms;

public static class LevenshteinCalculator
{
    public static int ComputeDistance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
            prev[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }
            Array.Copy(curr, prev, curr.Length);
        }

        return prev[b.Length];
    }

    /// <summary>Returns a 0–100 similarity score relative to the longer string's length.</summary>
    public static int SimilarityPercent(string a, string b)
    {
        var maxLength = Math.Max(a.Length, b.Length);
        if (maxLength == 0) return 100;
        return (int)Math.Round((1.0 - (double)ComputeDistance(a, b) / maxLength) * 100);
    }
}
