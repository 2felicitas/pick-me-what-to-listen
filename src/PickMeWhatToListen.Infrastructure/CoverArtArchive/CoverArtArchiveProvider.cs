using System.Net.Http.Json;
using PickMeWhatToListen.Application.Abstractions;

namespace PickMeWhatToListen.Infrastructure.CoverArtArchive;

public sealed class CoverArtArchiveProvider(
    HttpClient httpClient,
    CoverArtArchiveRateLimiter rateLimiter) : ICoverArtProvider
{
    internal const string ApiRoot = "https://coverartarchive.org/";

    public async Task<string?> GetReleaseGroupFrontThumbnail250UrlAsync(
        string releaseGroupMbid,
        CancellationToken cancellationToken = default)
    {
        await rateLimiter.WaitAsync(cancellationToken);
        var requestUri = new Uri($"{ApiRoot}release-group/{releaseGroupMbid}", UriKind.Absolute);
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (response.StatusCode is System.Net.HttpStatusCode.NotFound
            or System.Net.HttpStatusCode.BadRequest)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<CoverArtResponse>(cancellationToken);
        if (payload?.Images is null || payload.Images.Count == 0)
        {
            return null;
        }

        var front = payload.Images.FirstOrDefault(i => i.Front) ?? payload.Images[0];
        if (front.Thumbnails?.TryGetValue("250", out var url) == true)
        {
            return url;
        }

        return front.Image;
    }

    private sealed class CoverArtResponse
    {
        public List<CoverArtImage>? Images { get; init; }
    }

    private sealed class CoverArtImage
    {
        public bool Front { get; init; }

        public string? Image { get; init; }

        public Dictionary<string, string>? Thumbnails { get; init; }
    }
}
