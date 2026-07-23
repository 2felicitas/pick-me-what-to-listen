using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application;

/// <summary>
/// Core use cases: add an artist to the catalog, and pick a random one that
/// hasn't been picked yet.
/// </summary>
public sealed class ArtistCatalogService(IArtistRepository repository, IRandomProvider random)
{
    public Task<IReadOnlyList<Artist>> GetAllArtistsAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<Artist> AddArtistAsync(string name, CancellationToken cancellationToken = default)
    {
        var artist = Artist.Create(name);
        await repository.AddAsync(artist, cancellationToken);
        return artist;
    }

    public async Task<ArtistPickResult> PickRandomAsync(CancellationToken cancellationToken = default)
    {
        var unpicked = await repository.GetUnpickedAsync(cancellationToken);
        if (unpicked.Count == 0)
        {
            return ArtistPickResult.NoneLeft();
        }

        var index = random.Next(unpicked.Count);
        var chosen = unpicked[index];
        chosen.Pick();
        await repository.UpdateAsync(chosen, cancellationToken);

        return ArtistPickResult.Picked(chosen);
    }
}
