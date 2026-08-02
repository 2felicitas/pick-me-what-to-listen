namespace PickMeWhatToListen.Application.Abstractions;

public sealed record MusicBrainzArtistCandidate(
    string Mbid,
    string Name,
    string? Disambiguation,
    string? Type,
    string? Country,
    string? LifeSpanBegin,
    string? LifeSpanEnd);

public sealed record MusicBrainzVoteTerm(string Name, int Count);

public sealed record MusicBrainzReleaseGroupData(
    string Mbid,
    string Title,
    string PrimaryType,
    string? FirstReleaseDate,
    IReadOnlyList<string> SecondaryTypes);

public sealed record MusicBrainzReleaseData(string Mbid, string? Date);

public interface IArtistMetadataProvider
{
    public Task<IReadOnlyList<MusicBrainzArtistCandidate>> SearchArtistsAsync(
        string artistName,
        CancellationToken cancellationToken = default);

    public Task<(IReadOnlyList<MusicBrainzVoteTerm> Genres, IReadOnlyList<MusicBrainzVoteTerm> Tags, string? WikidataUrl)>
        GetArtistMetadataAsync(string artistMbid, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<MusicBrainzReleaseGroupData>> GetReleaseGroupsAsync(
        string artistMbid,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<MusicBrainzReleaseData>> GetReleasesAsync(
        string releaseGroupMbid,
        CancellationToken cancellationToken = default);
}
