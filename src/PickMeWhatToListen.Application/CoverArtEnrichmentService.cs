using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application;

public sealed class CoverArtEnrichmentService(
    IArtistMetadataRepository metadataRepository,
    ICoverArtProvider coverArtProvider,
    ICoverArtEnrichmentNotifier notifier)
{
    public async Task EnrichArtistCoversAsync(Guid artistId, CancellationToken cancellationToken = default)
    {
        var pending = await metadataRepository.GetPendingCoverArtAsync(artistId, cancellationToken);
        if (pending.Count == 0)
        {
            return;
        }

        foreach (var item in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string? coverUrl = null;
            try
            {
                coverUrl = await coverArtProvider.GetReleaseGroupFrontThumbnail250UrlAsync(
                    item.MusicBrainzReleaseGroupMbid,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Transient CAA failure — leave as None; a later enqueue can retry.
            }

            var status = coverUrl is null ? CoverArtStatus.Failed : CoverArtStatus.Ok;
            await metadataRepository.UpdateCoverArtAsync(
                item.ReleaseGroupId,
                coverUrl,
                status,
                cancellationToken);

            notifier.NotifyArtistCoverArtUpdated(artistId);
        }
    }
}
