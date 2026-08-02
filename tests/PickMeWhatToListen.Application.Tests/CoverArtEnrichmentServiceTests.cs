using PickMeWhatToListen.Application;
using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Application.Tests.TestDoubles;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application.Tests;

public sealed class CoverArtEnrichmentServiceTests
{
    [Fact]
    public async Task EnrichArtistCoversAsync_UpdatesPendingRowsAndNotifies()
    {
        const string RgMbid = "c9fdb94c-4975-4ed6-a96f-ef6d80bb7738";
        var artist = Artist.Create("The Beatles");
        artist.AssignMusicBrainzIdentity("b10bbbfc-cf9e-42e0-be17-e2c3e1d2600d", null);
        artist.MarkMetadataSynced();

        var releaseGroup = ReleaseGroup.Create(
            artist.Id,
            RgMbid,
            "Abbey Road",
            "Album",
            "1969-09-26",
            coverReleaseMbid: null,
            coverArtUrl: null,
            CoverArtStatus.None);

        var metadataRepository = new FakeArtistMetadataRepository();
        await metadataRepository.SaveAsync(
            new ArtistMetadataSaveRequest(artist, [], [releaseGroup]));

        Guid? notifiedArtistId = null;
        var notifier = new FakeCoverArtEnrichmentNotifier(id => notifiedArtistId = id);
        var coverArtProvider = new FakeCoverArtProvider("https://coverartarchive.org/250.jpg");
        var service = new CoverArtEnrichmentService(metadataRepository, coverArtProvider, notifier);

        await service.EnrichArtistCoversAsync(artist.Id);

        var profile = await metadataRepository.GetProfileAsync(artist.Id);
        Assert.Equal("https://coverartarchive.org/250.jpg", profile!.ReleaseGroups[0].CoverArtUrl);
        Assert.Equal(CoverArtStatus.Ok, profile.ReleaseGroups[0].CoverArtStatus);
        Assert.Equal(artist.Id, notifiedArtistId);
    }

    private sealed class FakeCoverArtProvider(string? url) : ICoverArtProvider
    {
        public Task<string?> GetReleaseGroupFrontThumbnail250UrlAsync(
            string releaseGroupMbid,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(url);
    }

    private sealed class FakeCoverArtEnrichmentNotifier(Action<Guid> onNotify) : ICoverArtEnrichmentNotifier
    {
        private Action<Guid>? _handler;

        public void Register(Action<Guid> handler) => _handler = handler;

        public void NotifyArtistCoverArtUpdated(Guid artistId) =>
            (_handler ?? onNotify)(artistId);
    }
}
