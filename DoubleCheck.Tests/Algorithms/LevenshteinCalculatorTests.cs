using DoubleCheck.Algorithms;
using FluentAssertions;

namespace DoubleCheck.Tests.Algorithms;

public class LevenshteinCalculatorTests
{
    // --- ComputeDistance ---

    [Fact]
    public void ComputeDistance_IdenticalStrings_ReturnsZero()
    {
        LevenshteinCalculator.ComputeDistance("bonjour", "bonjour").Should().Be(0);
    }

    [Fact]
    public void ComputeDistance_EmptyAndNonEmpty_ReturnsLengthOfNonEmpty()
    {
        LevenshteinCalculator.ComputeDistance("", "abc").Should().Be(3);
        LevenshteinCalculator.ComputeDistance("abc", "").Should().Be(3);
    }

    [Fact]
    public void ComputeDistance_BothEmpty_ReturnsZero()
    {
        LevenshteinCalculator.ComputeDistance("", "").Should().Be(0);
    }

    [Fact]
    public void ComputeDistance_ClassicKittenSitting_ReturnsThree()
    {
        // Référence canonique de l'algorithme
        LevenshteinCalculator.ComputeDistance("kitten", "sitting").Should().Be(3);
    }

    [Fact]
    public void ComputeDistance_SingleSubstitution_ReturnsOne()
    {
        LevenshteinCalculator.ComputeDistance("chat", "chат").Should().Be(0); // même string
        LevenshteinCalculator.ComputeDistance("Marie", "Mario").Should().Be(1);
    }

    [Fact]
    public void ComputeDistance_SingleInsertion_ReturnsOne()
    {
        LevenshteinCalculator.ComputeDistance("Beauchamp", "Beauchamps").Should().Be(1);
    }

    [Fact]
    public void ComputeDistance_SingleDeletion_ReturnsOne()
    {
        LevenshteinCalculator.ComputeDistance("Beauchamps", "Beauchamp").Should().Be(1);
    }

    [Fact]
    public void ComputeDistance_IsSymmetric()
    {
        var a = "Tremblay";
        var b = "Tremblai";
        LevenshteinCalculator.ComputeDistance(a, b)
            .Should().Be(LevenshteinCalculator.ComputeDistance(b, a));
    }

    [Theory]
    [InlineData("Nathalie", "Natalie",  1)]
    [InlineData("Philippe", "Philip",   3)]
    [InlineData("Côté",     "Coté",     1)]
    [InlineData("Sophie",   "Sophie",   0)]
    public void ComputeDistance_RealNameVariants_ReturnsExpectedDistance(
        string a, string b, int expected)
    {
        LevenshteinCalculator.ComputeDistance(a, b).Should().Be(expected);
    }

    // --- SimilarityPercent ---

    [Fact]
    public void SimilarityPercent_IdenticalStrings_Returns100()
    {
        LevenshteinCalculator.SimilarityPercent("Marie", "Marie").Should().Be(100);
    }

    [Fact]
    public void SimilarityPercent_BothEmpty_Returns100()
    {
        LevenshteinCalculator.SimilarityPercent("", "").Should().Be(100);
    }

    [Fact]
    public void SimilarityPercent_CompletelyDifferent_ReturnsLowScore()
    {
        LevenshteinCalculator.SimilarityPercent("abc", "xyz").Should().BeLessThan(50);
    }

    [Fact]
    public void SimilarityPercent_CloseNames_ReturnsHighScore()
    {
        // "Tremblay" vs "Tremblai" — 1 char de différence sur 8
        LevenshteinCalculator.SimilarityPercent("tremblay", "tremblai").Should().BeGreaterThan(85);
    }

    [Fact]
    public void SimilarityPercent_AlwaysBetweenZeroAndHundred()
    {
        var pairs = new[] { ("abc", "xyz"), ("", "hello"), ("same", "same"), ("a", "aaaaaaa") };
        foreach (var (a, b) in pairs)
            LevenshteinCalculator.SimilarityPercent(a, b).Should().BeInRange(0, 100);
    }
}
