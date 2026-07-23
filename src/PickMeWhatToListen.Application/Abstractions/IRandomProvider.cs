namespace PickMeWhatToListen.Application.Abstractions;

/// <summary>
/// Abstraction over randomness so <see cref="ArtistCatalogService"/> stays
/// deterministic and testable. The real implementation lives in Infrastructure.
/// </summary>
public interface IRandomProvider
{
    /// <summary>Returns a non-negative integer strictly less than <paramref name="exclusiveUpperBound"/>.</summary>
    public int Next(int exclusiveUpperBound);
}
