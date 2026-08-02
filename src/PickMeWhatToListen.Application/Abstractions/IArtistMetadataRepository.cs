using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application.Abstractions;

public sealed record MetadataTermEntry(string DisplayName, int VoteCount);

public sealed record ReleaseGroupEntry(
    string Title,
    string PrimaryType,
    string? FirstReleaseDate,
    string? CoverArtUrl,
    CoverArtStatus CoverArtStatus);

public sealed record PendingCoverArtItem(Guid ReleaseGroupId, string MusicBrainzReleaseGroupMbid);

public sealed record ArtistProfileSnapshot(
    Artist Artist,
    IReadOnlyList<MetadataTermEntry> Genres,
    IReadOnlyList<MetadataTermEntry> Tags,
    IReadOnlyList<ReleaseGroupEntry> ReleaseGroups);

public sealed record ArtistMetadataSaveRequest(
    Artist Artist,
    IReadOnlyList<(string DisplayName, MetadataTermKind Kind, int VoteCount)> Terms,
    IReadOnlyList<ReleaseGroup> ReleaseGroups);

public interface IArtistMetadataRepository
{
    public Task<ArtistProfileSnapshot?> GetProfileAsync(Guid artistId, CancellationToken cancellationToken = default);

    public Task SaveAsync(ArtistMetadataSaveRequest request, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<PendingCoverArtItem>> GetPendingCoverArtAsync(
        Guid artistId,
        CancellationToken cancellationToken = default);

    public Task UpdateCoverArtAsync(
        Guid releaseGroupId,
        string? coverArtUrl,
        CoverArtStatus coverArtStatus,
        CancellationToken cancellationToken = default);
}
