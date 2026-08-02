namespace PickMeWhatToListen.Infrastructure;

/// <summary>
/// User-Agent for outbound HTTP clients (MusicBrainz, Cover Art Archive).
/// Set once from the WPF composition root before <see cref="ServiceCollectionExtensions.AddInfrastructure"/>.
/// </summary>
public static class HttpClientUserAgent
{
    private const string DefaultVersion = "0.1.0";
    private const string DefaultRepositoryUrl = "https://github.com/2felicitas/pick-me-what-to-listen";

    public static string Value { get; private set; } =
        $"PickMeWhatToListen/{DefaultVersion} ({DefaultRepositoryUrl})";

    public static void Configure(string version, string repositoryUrl)
    {
        Value = $"PickMeWhatToListen/{version} ({repositoryUrl})";
    }
}
