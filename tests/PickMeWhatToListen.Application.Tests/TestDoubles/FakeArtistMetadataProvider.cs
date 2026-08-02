using PickMeWhatToListen.Application.Abstractions;

namespace PickMeWhatToListen.Application.Tests.TestDoubles;

public sealed class FakeArtistMetadataProvider : IArtistMetadataProvider
{
    private readonly Dictionary<string, IReadOnlyList<MusicBrainzArtistCandidate>> _searchResults = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (IReadOnlyList<MusicBrainzVoteTerm> Genres, IReadOnlyList<MusicBrainzVoteTerm> Tags, string? WikidataUrl)> _artistMetadata = [];
    private readonly Dictionary<string, IReadOnlyList<MusicBrainzReleaseGroupData>> _releaseGroups = [];
    private readonly Dictionary<string, IReadOnlyList<MusicBrainzReleaseData>> _releases = [];

    public FakeArtistMetadataProvider SeedSearch(string artistName, params MusicBrainzArtistCandidate[] candidates)
    {
        _searchResults[artistName] = candidates;
        return this;
    }

    public FakeArtistMetadataProvider SeedArtist(
        string mbid,
        IReadOnlyList<MusicBrainzVoteTerm> genres,
        IReadOnlyList<MusicBrainzVoteTerm> tags,
        string? wikidataUrl = null)
    {
        _artistMetadata[mbid] = (genres, tags, wikidataUrl);
        return this;
    }

    public FakeArtistMetadataProvider SeedReleaseGroups(string artistMbid, params MusicBrainzReleaseGroupData[] groups)
    {
        _releaseGroups[artistMbid] = groups;
        return this;
    }

    public FakeArtistMetadataProvider SeedReleases(string releaseGroupMbid, params MusicBrainzReleaseData[] releases)
    {
        _releases[releaseGroupMbid] = releases;
        return this;
    }

    public Task<IReadOnlyList<MusicBrainzArtistCandidate>> SearchArtistsAsync(
        string artistName,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_searchResults.GetValueOrDefault(artistName) ?? []);

    public Task<(IReadOnlyList<MusicBrainzVoteTerm> Genres, IReadOnlyList<MusicBrainzVoteTerm> Tags, string? WikidataUrl)>
        GetArtistMetadataAsync(string artistMbid, CancellationToken cancellationToken = default) =>
        Task.FromResult(_artistMetadata.TryGetValue(artistMbid, out var data)
            ? data
            : ([], [], null));

    public Task<IReadOnlyList<MusicBrainzReleaseGroupData>> GetReleaseGroupsAsync(
        string artistMbid,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_releaseGroups.GetValueOrDefault(artistMbid) ?? []);

    public Task<IReadOnlyList<MusicBrainzReleaseData>> GetReleasesAsync(
        string releaseGroupMbid,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_releases.GetValueOrDefault(releaseGroupMbid) ?? []);
}
