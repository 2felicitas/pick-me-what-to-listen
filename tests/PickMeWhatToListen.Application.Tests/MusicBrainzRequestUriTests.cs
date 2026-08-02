namespace PickMeWhatToListen.Application.Tests;

/// <summary>
/// Guards the HttpClient relative-URI gotcha documented in MusicBrainzMetadataProvider.
/// </summary>
public sealed class MusicBrainzRequestUriTests
{
    private const string ApiRoot = "https://musicbrainz.org/ws/2/";

    [Theory]
    [InlineData("artist?query=test&fmt=json&limit=5", "/ws/2/artist?")]
    [InlineData("artist/b10bbbfc-cf9e-42e0-be17-e2c3e1d2600d?inc=genres+tags+url-rels&fmt=json", "/ws/2/artist/b10bbbfc")]
    [InlineData("release-group?artist=b10bbbfc&type=album|ep&fmt=json", "/ws/2/release-group?")]
    public void BuildRequestUri_KeepsWs2Segment(string pathAndQuery, string expectedSegment)
    {
        var uri = BuildRequestUri(pathAndQuery);

        Assert.Contains(expectedSegment, uri.AbsoluteUri, StringComparison.Ordinal);
        Assert.DoesNotContain("/ws/artist?", uri.AbsoluteUri, StringComparison.Ordinal);
        Assert.StartsWith(ApiRoot, uri.AbsoluteUri, StringComparison.Ordinal);
    }

    private static Uri BuildRequestUri(string pathAndQuery)
    {
        var baseUri = new Uri(ApiRoot, UriKind.Absolute);
        var relative = pathAndQuery.StartsWith("./", StringComparison.Ordinal)
            ? pathAndQuery
            : "./" + pathAndQuery.TrimStart('/');
        return new Uri(baseUri, relative);
    }
}
