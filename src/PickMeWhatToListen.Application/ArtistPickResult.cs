using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application;

/// <summary>
/// Outcome of <see cref="ArtistCatalogService.PickRandomAsync"/>. Modeled as a
/// result instead of an exception because "nothing left to pick" is an
/// expected, user-facing condition, not an error.
/// </summary>
public sealed class ArtistPickResult
{
    private ArtistPickResult(Artist? artist)
    {
        Artist = artist;
    }

    public Artist? Artist { get; }

    public bool Succeeded => Artist is not null;

    public static ArtistPickResult Picked(Artist artist) => new(artist);

    public static ArtistPickResult NoneLeft() => new(null);
}
