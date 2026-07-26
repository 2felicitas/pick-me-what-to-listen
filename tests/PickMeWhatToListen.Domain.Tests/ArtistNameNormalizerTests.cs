namespace PickMeWhatToListen.Domain.Tests;

public class ArtistNameNormalizerTests
{
    [Theory]
    [InlineData("Aphex Twin", "aphex twin")]
    [InlineData("APHEX TWIN", "aphex twin")]
    [InlineData("  Aphex   Twin  ", "aphex twin")]
    [InlineData("Мёбиус", "мебиус")]
    [InlineData("Şevval", "sevval")]
    [InlineData("O'Brien", "obrien")]
    [InlineData("O’Brien", "obrien")]
    [InlineData("O`Brien", "obrien")]
    public void ToComparisonKey_TreatsEquivalentSpellingsTheSame(string name, string expectedKey)
    {
        Assert.Equal(expectedKey, ArtistNameNormalizer.ToComparisonKey(name));
    }

    [Theory]
    [InlineData("Мёбиус", "Мебиус")]
    [InlineData("Şevval", "Sevval")]
    [InlineData("O'Brien", "O’Brien")]
    [InlineData("aphex twin", "APHEX TWIN")]
    [InlineData("Boards of Canada", "  Boards   of   Canada ")]
    public void ToComparisonKey_MatchesForKnownEquivalentPairs(string first, string second)
    {
        Assert.Equal(ArtistNameNormalizer.ToComparisonKey(first), ArtistNameNormalizer.ToComparisonKey(second));
    }

    [Fact]
    public void ToComparisonKey_DoesNotUnifyNonDiacriticLookAlikes()
    {
        // Documented limitation: dotless Turkish "ı" is a distinct base letter
        // in Unicode, not "i" plus a combining mark, so NFD decomposition
        // can't unify it the way it does for ё/е or ş/s. See ARCHITECTURE.md /
        // docs/product-specs/bulk-import.md.
        Assert.NotEqual(ArtistNameNormalizer.ToComparisonKey("Sıla"), ArtistNameNormalizer.ToComparisonKey("Sila"));
    }

    [Fact]
    public void ToComparisonKey_DistinguishesActuallyDifferentNames()
    {
        Assert.NotEqual(ArtistNameNormalizer.ToComparisonKey("Autechre"), ArtistNameNormalizer.ToComparisonKey("Aphex Twin"));
    }
}
