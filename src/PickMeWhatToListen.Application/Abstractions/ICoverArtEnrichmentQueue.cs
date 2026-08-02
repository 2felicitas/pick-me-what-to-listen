namespace PickMeWhatToListen.Application.Abstractions;

/// <summary>Schedules Cover Art Archive lookups without blocking profile sync or UI.</summary>
public interface ICoverArtEnrichmentQueue
{
    public void EnqueueArtist(Guid artistId);
}
