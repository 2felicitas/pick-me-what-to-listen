using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PickMeWhatToListen.Application.Abstractions;

namespace PickMeWhatToListen.Infrastructure.MusicBrainz;

public sealed class MusicBrainzMetadataProvider(
    HttpClient httpClient,
    MusicBrainzRateLimiter rateLimiter) : IArtistMetadataProvider
{
    internal const string ApiRoot = "https://musicbrainz.org/ws/2/";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<IReadOnlyList<MusicBrainzArtistCandidate>> SearchArtistsAsync(
        string artistName,
        CancellationToken cancellationToken = default)
    {
        var escaped = Uri.EscapeDataString($"artist:\"{artistName}\"");
        var url = $"artist?query={escaped}&fmt=json&limit=5";
        var response = await GetJsonAsync<ArtistSearchResponse>(url, cancellationToken);

        return response?.Artists?
            .Select(MapCandidate)
            .ToList() ?? [];
    }

    public async Task<(IReadOnlyList<MusicBrainzVoteTerm> Genres, IReadOnlyList<MusicBrainzVoteTerm> Tags, string? WikidataUrl)>
        GetArtistMetadataAsync(string artistMbid, CancellationToken cancellationToken = default)
    {
        var url = $"artist/{artistMbid}?inc=genres+tags+url-rels&fmt=json";
        var response = await GetJsonAsync<ArtistDetailResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException($"MusicBrainz artist '{artistMbid}' not found.");

        var genres = MapVoteTerms(response.Genres);
        var tags = MapVoteTerms(response.Tags);
        var wikidataUrl = response.Relations?
            .FirstOrDefault(r => string.Equals(r.Type, "wikidata", StringComparison.OrdinalIgnoreCase))
            ?.Url?.Resource;

        return (genres, tags, wikidataUrl);
    }

    public async Task<IReadOnlyList<MusicBrainzReleaseGroupData>> GetReleaseGroupsAsync(
        string artistMbid,
        CancellationToken cancellationToken = default)
    {
        return await BrowseReleaseGroupsAsync(artistMbid, "album|ep", cancellationToken);
    }

    public async Task<IReadOnlyList<MusicBrainzReleaseData>> GetReleasesAsync(
        string releaseGroupMbid,
        CancellationToken cancellationToken = default)
    {
        var results = new List<MusicBrainzReleaseData>();
        var offset = 0;
        const int limit = 100;

        while (true)
        {
            var url = $"release?release-group={releaseGroupMbid}&fmt=json&limit={limit}&offset={offset}";
            var response = await GetJsonAsync<ReleaseBrowseResponse>(url, cancellationToken);
            var releases = response?.Releases ?? [];

            results.AddRange(releases.Select(r => new MusicBrainzReleaseData(r.Id, r.Date)));

            if (releases.Count < limit)
            {
                break;
            }

            offset += limit;
        }

        return results;
    }

    private async Task<IReadOnlyList<MusicBrainzReleaseGroupData>> BrowseReleaseGroupsAsync(
        string artistMbid,
        string primaryType,
        CancellationToken cancellationToken)
    {
        var results = new List<MusicBrainzReleaseGroupData>();
        var offset = 0;
        const int limit = 100;

        while (true)
        {
            var url =
                $"release-group?artist={artistMbid}&type={primaryType}&fmt=json&limit={limit}&offset={offset}";
            var response = await GetJsonAsync<ReleaseGroupBrowseResponse>(url, cancellationToken);
            var groups = response?.ReleaseGroups ?? [];

            results.AddRange(groups.Select(g => new MusicBrainzReleaseGroupData(
                g.Id,
                g.Title,
                g.PrimaryType ?? "Album",
                g.FirstReleaseDate,
                g.SecondaryTypes ?? [])));

            if (groups.Count < limit)
            {
                break;
            }

            offset += limit;
        }

        return results;
    }

    private async Task<T?> GetJsonAsync<T>(string pathAndQuery, CancellationToken cancellationToken)
    {
        await rateLimiter.WaitAsync(cancellationToken);
        using var response = await httpClient.GetAsync(BuildRequestUri(pathAndQuery), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"MusicBrainz request failed ({(int)response.StatusCode} {response.ReasonPhrase}) " +
                $"for '{BuildRequestUri(pathAndQuery)}'. {body}".Trim());
        }

        return await response.Content.ReadFromJsonAsync<T>(s_jsonOptions, cancellationToken);
    }

    internal static Uri BuildRequestUri(string pathAndQuery)
    {
        // HttpClient BaseAddress + relative "artist?…" drops the trailing "2/" segment and hits
        // /ws/artist instead of /ws/2/artist. Always build from ApiRoot; "./" keeps the base path.
        var baseUri = new Uri(ApiRoot, UriKind.Absolute);
        var relative = pathAndQuery.StartsWith("./", StringComparison.Ordinal)
            ? pathAndQuery
            : "./" + pathAndQuery.TrimStart('/');
        return new Uri(baseUri, relative);
    }

    private static MusicBrainzArtistCandidate MapCandidate(ArtistSearchItem item) =>
        new(
            item.Id,
            item.Name,
            item.Disambiguation,
            item.Type,
            item.Country,
            item.LifeSpan?.Begin,
            item.LifeSpan?.End);

    private static IReadOnlyList<MusicBrainzVoteTerm> MapVoteTerms(IReadOnlyList<VoteTermItem>? items) =>
        items?.Select(i => new MusicBrainzVoteTerm(i.Name, i.Count)).ToList() ?? [];

    private sealed class ArtistSearchResponse
    {
        public List<ArtistSearchItem>? Artists { get; init; }
    }

    private sealed class ArtistSearchItem
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string? Disambiguation { get; init; }

        public string? Type { get; init; }

        public string? Country { get; init; }

        public LifeSpanItem? LifeSpan { get; init; }
    }

    private sealed class LifeSpanItem
    {
        public string? Begin { get; init; }

        public string? End { get; init; }
    }

    private sealed class ArtistDetailResponse
    {
        public List<VoteTermItem>? Genres { get; init; }

        public List<VoteTermItem>? Tags { get; init; }

        public List<RelationItem>? Relations { get; init; }
    }

    private sealed class VoteTermItem
    {
        public string Name { get; init; } = string.Empty;

        public int Count { get; init; }
    }

    private sealed class RelationItem
    {
        public string? Type { get; init; }

        public UrlItem? Url { get; init; }
    }

    private sealed class UrlItem
    {
        public string? Resource { get; init; }
    }

    private sealed class ReleaseGroupBrowseResponse
    {
        [JsonPropertyName("release-groups")]
        public List<ReleaseGroupItem>? ReleaseGroups { get; init; }
    }

    private sealed class ReleaseGroupItem
    {
        public string Id { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("primary-type")]
        public string? PrimaryType { get; init; }

        [JsonPropertyName("first-release-date")]
        public string? FirstReleaseDate { get; init; }

        [JsonPropertyName("secondary-types")]
        public List<string>? SecondaryTypes { get; init; }
    }

    private sealed class ReleaseBrowseResponse
    {
        public List<ReleaseItem>? Releases { get; init; }
    }

    private sealed class ReleaseItem
    {
        public string Id { get; init; } = string.Empty;

        public string? Date { get; init; }
    }
}
