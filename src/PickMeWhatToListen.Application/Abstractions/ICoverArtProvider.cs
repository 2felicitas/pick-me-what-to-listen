namespace PickMeWhatToListen.Application.Abstractions;

public interface ICoverArtProvider
{
    public Task<string?> GetReleaseGroupFrontThumbnail250UrlAsync(
        string releaseGroupMbid,
        CancellationToken cancellationToken = default);
}
