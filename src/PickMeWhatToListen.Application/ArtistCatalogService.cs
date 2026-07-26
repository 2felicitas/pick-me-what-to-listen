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

    public async Task<ArtistAddResult> AddArtistAsync(string name, CancellationToken cancellationToken = default)
    {
        var candidate = Artist.Create(name);
        var existing = await repository.GetAllAsync(cancellationToken);

        var duplicate = FindDuplicate(candidate.Name, existing);
        if (duplicate is not null)
        {
            return ArtistAddResult.Duplicate(duplicate);
        }

        await repository.AddAsync(candidate, cancellationToken);
        return ArtistAddResult.Added(candidate);
    }

    /// <summary>
    /// Adds every non-blank, non-duplicate name from <paramref name="names"/>.
    /// Invalid lines (empty after trimming, over <see cref="Artist.MaxNameLength"/>)
    /// and duplicates (against the existing catalog or an earlier line in the
    /// same batch) are skipped rather than aborting the whole import — see
    /// docs/product-specs/bulk-import.md.
    /// </summary>
    public async Task<BulkAddArtistsResult> AddArtistsAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetAllAsync(cancellationToken);
        var knownKeys = new HashSet<string>(existing.Select(a => ArtistNameNormalizer.ToComparisonKey(a.Name)));

        var addedCount = 0;
        var skippedDuplicateCount = 0;
        var skippedInvalidCount = 0;

        foreach (var rawName in names)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                continue;
            }

            Artist candidate;
            try
            {
                candidate = Artist.Create(rawName);
            }
            catch (ArgumentException)
            {
                skippedInvalidCount++;
                continue;
            }

            if (!knownKeys.Add(ArtistNameNormalizer.ToComparisonKey(candidate.Name)))
            {
                skippedDuplicateCount++;
                continue;
            }

            await repository.AddAsync(candidate, cancellationToken);
            addedCount++;
        }

        return new BulkAddArtistsResult(addedCount, skippedDuplicateCount, skippedInvalidCount);
    }

    private static Artist? FindDuplicate(string name, IReadOnlyList<Artist> existingArtists)
    {
        var key = ArtistNameNormalizer.ToComparisonKey(name);
        return existingArtists.FirstOrDefault(a => ArtistNameNormalizer.ToComparisonKey(a.Name) == key);
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
