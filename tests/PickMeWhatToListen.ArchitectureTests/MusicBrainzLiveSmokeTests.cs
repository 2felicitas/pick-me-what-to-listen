using Microsoft.EntityFrameworkCore;
using PickMeWhatToListen.Infrastructure;
using PickMeWhatToListen.Infrastructure.MusicBrainz;

namespace PickMeWhatToListen.ArchitectureTests;

/// <summary>Live smoke test against MusicBrainz — skipped in CI by default.</summary>
public sealed class MusicBrainzLiveSmokeTests
{
    [Fact(Skip = "Live network call — run manually when debugging MusicBrainz integration.")]
    public async Task SearchAndLookup_DoNotReturnServerError()
    {
        var handler = new HttpClientHandler();
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "PickMeWhatToListen/1.0 (https://github.com/local/pick-me-what-to-listen)");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

        var provider = new MusicBrainzMetadataProvider(client, new MusicBrainzRateLimiter());

        var candidates = await provider.SearchArtistsAsync("Radiohead");
        Assert.NotEmpty(candidates);

        var (genres, tags, wikidata) = await provider.GetArtistMetadataAsync(candidates[0].Mbid);
        _ = genres;
        _ = tags;
        _ = wikidata;
        var groups = await provider.GetReleaseGroupsAsync(candidates[0].Mbid);
        Assert.NotEmpty(groups);
    }
}
