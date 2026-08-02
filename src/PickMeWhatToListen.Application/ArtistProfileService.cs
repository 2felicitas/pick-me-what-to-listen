using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application;

public sealed record ArtistProfileResult(
    bool Found,
    ArtistProfileSnapshot? Profile,
    IReadOnlyList<MusicBrainzArtistCandidate> AmbiguousCandidates)
{
    public static ArtistProfileResult NotFound() => new(false, null, []);

    public static ArtistProfileResult Ok(ArtistProfileSnapshot profile) => new(true, profile, []);

    public static ArtistProfileResult Ambiguous(
        ArtistProfileSnapshot profile,
        IReadOnlyList<MusicBrainzArtistCandidate> candidates) =>
        new(true, profile, candidates);
}

public sealed class ArtistProfileService(
    IArtistRepository artistRepository,
    IArtistMetadataRepository metadataRepository,
    IArtistMetadataProvider metadataProvider,
    ICoverArtEnrichmentQueue coverArtEnrichmentQueue)
{
    public static readonly TimeSpan MetadataTtl = TimeSpan.FromDays(30);

    public async Task<ArtistProfileResult> GetProfileAsync(
        Guid artistId,
        CancellationToken cancellationToken = default)
    {
        var artist = await artistRepository.GetByIdAsync(artistId, cancellationToken);
        if (artist is null)
        {
            return ArtistProfileResult.NotFound();
        }

        if (NeedsRefresh(artist))
        {
            var refreshResult = await RefreshMetadataAsync(artist, cancellationToken);
            artist = refreshResult.Artist ?? artist;

            var snapshot = await metadataRepository.GetProfileAsync(artistId, cancellationToken)
                ?? new ArtistProfileSnapshot(artist, [], [], []);

            if (refreshResult.AmbiguousCandidates.Count > 0)
            {
                return ArtistProfileResult.Ambiguous(snapshot, refreshResult.AmbiguousCandidates);
            }

            return ArtistProfileResult.Ok(snapshot);
        }

        var cached = await metadataRepository.GetProfileAsync(artistId, cancellationToken);
        if (cached is null)
        {
            return ArtistProfileResult.Ok(new ArtistProfileSnapshot(artist, [], [], []));
        }

        coverArtEnrichmentQueue.EnqueueArtist(artistId);
        return ArtistProfileResult.Ok(cached);
    }

    public async Task<ArtistProfileResult?> GetCachedProfileAsync(
        Guid artistId,
        CancellationToken cancellationToken = default)
    {
        var artist = await artistRepository.GetByIdAsync(artistId, cancellationToken);
        if (artist is null)
        {
            return null;
        }

        var cached = await metadataRepository.GetProfileAsync(artistId, cancellationToken);
        return cached is null
            ? ArtistProfileResult.Ok(new ArtistProfileSnapshot(artist, [], [], []))
            : ArtistProfileResult.Ok(cached);
    }

    public async Task<ArtistProfileResult> ConfirmMusicBrainzMatchAsync(
        Guid artistId,
        string mbid,
        CancellationToken cancellationToken = default)
    {
        var artist = await artistRepository.GetByIdAsync(artistId, cancellationToken);
        if (artist is null)
        {
            return ArtistProfileResult.NotFound();
        }

        artist.AssignMusicBrainzIdentity(mbid, wikidataUrl: null);
        artist.SetMetadataSyncStatus(MetadataSyncStatus.None);
        await artistRepository.UpdateAsync(artist, cancellationToken);

        return await GetProfileAsync(artistId, cancellationToken);
    }

    public async Task<ArtistProfileResult> ForceRefreshAsync(
        Guid artistId,
        CancellationToken cancellationToken = default)
    {
        var artist = await artistRepository.GetByIdAsync(artistId, cancellationToken);
        if (artist is null)
        {
            return ArtistProfileResult.NotFound();
        }

        artist.SetMetadataSyncStatus(MetadataSyncStatus.None);
        await artistRepository.UpdateAsync(artist, cancellationToken);

        return await GetProfileAsync(artistId, cancellationToken);
    }

    private static bool NeedsRefresh(Artist artist) =>
        artist.MetadataSyncStatus is MetadataSyncStatus.None
            or MetadataSyncStatus.Failed
            or MetadataSyncStatus.NotFound
            or MetadataSyncStatus.Ambiguous
        || artist.MetadataSyncedAtUtc is null
        || DateTimeOffset.UtcNow - artist.MetadataSyncedAtUtc.Value > MetadataTtl;

    private async Task<(Artist? Artist, IReadOnlyList<MusicBrainzArtistCandidate> AmbiguousCandidates)> RefreshMetadataAsync(
        Artist artist,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(artist.MusicBrainzArtistMbid))
            {
                var candidates = await metadataProvider.SearchArtistsAsync(artist.Name, cancellationToken);
                if (candidates.Count == 0)
                {
                    artist.SetMetadataSyncStatus(MetadataSyncStatus.NotFound, "Исполнитель не найден в MusicBrainz.");
                    await artistRepository.UpdateAsync(artist, cancellationToken);
                    return (artist, []);
                }

                if (candidates.Count > 1)
                {
                    artist.SetMetadataSyncStatus(MetadataSyncStatus.Ambiguous);
                    await artistRepository.UpdateAsync(artist, cancellationToken);
                    return (artist, candidates);
                }

                artist.AssignMusicBrainzIdentity(candidates[0].Mbid, wikidataUrl: null);
            }

            var (genres, tags, wikidataUrl) =
                await metadataProvider.GetArtistMetadataAsync(artist.MusicBrainzArtistMbid!, cancellationToken);
            artist.AssignMusicBrainzIdentity(artist.MusicBrainzArtistMbid!, wikidataUrl);

            var releaseGroupsData = ArtistMetadataRules.SelectDiscographyForSync(
                await metadataProvider.GetReleaseGroupsAsync(artist.MusicBrainzArtistMbid!, cancellationToken));

            var terms = ArtistMetadataRules.BuildTerms(genres, tags);
            var releaseGroups = releaseGroupsData
                .Select(group => ReleaseGroup.Create(
                    artist.Id,
                    group.Mbid,
                    group.Title,
                    group.PrimaryType,
                    group.FirstReleaseDate,
                    coverReleaseMbid: null,
                    coverArtUrl: null,
                    CoverArtStatus.None))
                .ToList();

            artist.MarkMetadataSynced();
            await metadataRepository.SaveAsync(
                new ArtistMetadataSaveRequest(artist, terms, releaseGroups),
                cancellationToken);

            coverArtEnrichmentQueue.EnqueueArtist(artist.Id);

            return (artist, []);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            artist.SetMetadataSyncStatus(MetadataSyncStatus.Failed, ex.Message);
            await artistRepository.UpdateAsync(artist, cancellationToken);
            return (artist, []);
        }
    }
}
