namespace PickMeWhatToListen.Application.Abstractions;

/// <summary>Signals that background cover-art enrichment updated an artist's cached discography.</summary>
public interface ICoverArtEnrichmentNotifier
{
    public void Register(Action<Guid> handler);

    public void NotifyArtistCoverArtUpdated(Guid artistId);
}
