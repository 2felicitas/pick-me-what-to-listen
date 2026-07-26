using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application;

/// <summary>
/// Outcome of <see cref="ArtistCatalogService.AddArtistAsync"/>. Modeled as a
/// result instead of an exception because "this artist is already in the
/// catalog" is an expected, user-facing condition, not an error — see
/// <see cref="ArtistPickResult"/> for the same pattern.
/// </summary>
public sealed class ArtistAddResult
{
    private ArtistAddResult(Artist? artist, Artist? duplicateOf)
    {
        Artist = artist;
        DuplicateOf = duplicateOf;
    }

    public Artist? Artist { get; }

    /// <summary>The existing catalog entry that made this add a duplicate, if any.</summary>
    public Artist? DuplicateOf { get; }

    public bool Succeeded => Artist is not null;

    public static ArtistAddResult Added(Artist artist) => new(artist, duplicateOf: null);

    public static ArtistAddResult Duplicate(Artist existingMatch) => new(artist: null, existingMatch);
}
