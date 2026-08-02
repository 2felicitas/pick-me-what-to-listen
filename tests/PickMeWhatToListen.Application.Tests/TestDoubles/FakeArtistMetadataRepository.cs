using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application.Tests.TestDoubles;

public sealed class FakeArtistMetadataRepository : IArtistMetadataRepository
{
    private readonly Dictionary<Guid, ArtistMetadataSaveRequest> _profiles = [];

    public Task<ArtistProfileSnapshot?> GetProfileAsync(Guid artistId, CancellationToken cancellationToken = default)
    {
        if (!_profiles.TryGetValue(artistId, out var saved))
        {
            return Task.FromResult<ArtistProfileSnapshot?>(null);
        }

        var genres = saved.Terms
            .Where(t => t.Kind == MetadataTermKind.Genre)
            .Select(t => new MetadataTermEntry(t.DisplayName, t.VoteCount))
            .ToList();

        var tags = saved.Terms
            .Where(t => t.Kind == MetadataTermKind.Tag)
            .Select(t => new MetadataTermEntry(t.DisplayName, t.VoteCount))
            .ToList();

        var releaseGroups = saved.ReleaseGroups
            .Select(r => new ReleaseGroupEntry(
                r.Title,
                r.PrimaryType,
                r.FirstReleaseDate,
                r.CoverArtUrl,
                r.CoverArtStatus))
            .ToList();

        return Task.FromResult<ArtistProfileSnapshot?>(
            new ArtistProfileSnapshot(saved.Artist, genres, tags, releaseGroups));
    }

    public Task SaveAsync(ArtistMetadataSaveRequest request, CancellationToken cancellationToken = default)
    {
        _profiles[request.Artist.Id] = request;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PendingCoverArtItem>> GetPendingCoverArtAsync(
        Guid artistId,
        CancellationToken cancellationToken = default)
    {
        if (!_profiles.TryGetValue(artistId, out var saved))
        {
            return Task.FromResult<IReadOnlyList<PendingCoverArtItem>>([]);
        }

        var pending = saved.ReleaseGroups
            .Where(r => r.CoverArtStatus == CoverArtStatus.None)
            .Select(r => new PendingCoverArtItem(r.Id, r.MusicBrainzReleaseGroupMbid))
            .ToList();

        return Task.FromResult<IReadOnlyList<PendingCoverArtItem>>(pending);
    }

    public Task UpdateCoverArtAsync(
        Guid releaseGroupId,
        string? coverArtUrl,
        CoverArtStatus coverArtStatus,
        CancellationToken cancellationToken = default)
    {
        foreach (var saved in _profiles.Values)
        {
            var releaseGroup = saved.ReleaseGroups.FirstOrDefault(r => r.Id == releaseGroupId);
            if (releaseGroup is null)
            {
                continue;
            }

            releaseGroup.SetCoverArt(coverArtUrl, coverArtStatus);
            break;
        }

        return Task.CompletedTask;
    }
}
