using PickMeWhatToListen.Application;
using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Application.Tests.TestDoubles;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application.Tests;

public sealed class ArtistProfileServiceTests
{
    [Fact]
    public void SelectDiscographyForSync_ExcludesReleaseGroupsWithSecondaryTypes()
    {
        var groups = new[]
        {
            new MusicBrainzReleaseGroupData("studio", "Studio Album", "Album", "2020", []),
            new MusicBrainzReleaseGroupData("live", "Live at Wembley", "Album", "2019", ["Live"]),
            new MusicBrainzReleaseGroupData("comp", "Greatest Hits", "Album", "2018", ["Compilation"]),
        };

        var selected = ArtistMetadataRules.SelectDiscographyForSync(groups);

        Assert.Single(selected);
        Assert.Equal("Studio Album", selected[0].Title);
    }

    [Fact]
    public void SelectDiscographyForSync_KeepsNewestRowsUpToCap()
    {
        var groups = Enumerable.Range(1, 60)
            .Select(i => new MusicBrainzReleaseGroupData(
                $"mbid-{i}",
                $"Album {i}",
                "Album",
                $"{1960 + i}-01-01",
                []))
            .ToList();

        var selected = ArtistMetadataRules.SelectDiscographyForSync(groups);

        Assert.Equal(ArtistMetadataRules.MaxReleaseGroups, selected.Count);
        Assert.Equal("Album 60", selected[0].Title);
        Assert.Equal("Album 11", selected[^1].Title);
    }

    [Fact]
    public void BuildTerms_UsesGenresAndTagsMinusGenres_WithMinVoteCount()
    {
        var genres = new[]
        {
            new MusicBrainzVoteTerm("Rock", 10),
            new MusicBrainzVoteTerm("Pop", 3),
        };

        var tags = new[]
        {
            new MusicBrainzVoteTerm("Rock", 8),
            new MusicBrainzVoteTerm("British", 6),
            new MusicBrainzVoteTerm("Obscure", 4),
        };

        var terms = ArtistMetadataRules.BuildTerms(genres, tags);

        Assert.Equal(2, terms.Count);
        Assert.Contains(terms, t => t.DisplayName == "Rock" && t.Kind == MetadataTermKind.Genre);
        Assert.Contains(terms, t => t.DisplayName == "British" && t.Kind == MetadataTermKind.Tag);
        Assert.DoesNotContain(terms, t => t.DisplayName == "Pop");
        Assert.DoesNotContain(terms, t => t.DisplayName == "Obscure");
        Assert.DoesNotContain(terms, t => t.DisplayName == "Rock" && t.Kind == MetadataTermKind.Tag);
    }

    [Fact]
    public void PickEarliestRelease_PrefersOldestDate()
    {
        var releases = new[]
        {
            new MusicBrainzReleaseData("newer", "2005-01-01"),
            new MusicBrainzReleaseData("older", "1999"),
            new MusicBrainzReleaseData("undated", null),
        };

        var picked = ArtistMetadataRules.PickEarliestRelease(releases);

        Assert.Equal("older", picked!.Mbid);
    }

    [Fact]
    public async Task GetProfileAsync_AutoLinksSingleSearchHitAndPersistsMetadata()
    {
        const string Mbid = "b10bbbfc-cf9e-42e0-be17-e2c3e1d2600d";
        const string RgMbid = "c9fdb94c-4975-4ed6-a96f-ef6d80bb7738";

        var artist = Artist.Create("The Beatles");
        var artistRepository = new FakeArtistRepository().Seed(artist);
        var metadataRepository = new FakeArtistMetadataRepository();
        var metadataProvider = new FakeArtistMetadataProvider()
            .SeedSearch("The Beatles", new MusicBrainzArtistCandidate(Mbid, "The Beatles", null, null, null, null, null))
            .SeedArtist(
                Mbid,
                [new MusicBrainzVoteTerm("Rock", 10)],
                [new MusicBrainzVoteTerm("British", 6)],
                "https://www.wikidata.org/wiki/Q1299")
            .SeedReleaseGroups(
                Mbid,
                new MusicBrainzReleaseGroupData(RgMbid, "Abbey Road", "Album", "1969-09-26", []))
            .SeedReleases(RgMbid, new MusicBrainzReleaseData("release-1", "1969-09-26"));

        var coverArtQueue = new FakeCoverArtEnrichmentQueue();
        var service = new ArtistProfileService(artistRepository, metadataRepository, metadataProvider, coverArtQueue);

        var result = await service.GetProfileAsync(artist.Id);

        Assert.True(result.Found);
        Assert.NotNull(result.Profile);
        Assert.Equal(Mbid, result.Profile!.Artist.MusicBrainzArtistMbid);
        Assert.Equal("https://www.wikidata.org/wiki/Q1299", result.Profile.Artist.WikidataUrl);
        Assert.Single(result.Profile.Genres);
        Assert.Single(result.Profile.Tags);
        Assert.Single(result.Profile.ReleaseGroups);
        Assert.Null(result.Profile.ReleaseGroups[0].CoverArtUrl);
        Assert.Equal(CoverArtStatus.None, result.Profile.ReleaseGroups[0].CoverArtStatus);
        Assert.Equal(artist.Id, coverArtQueue.EnqueuedArtistIds.Single());
    }

    private sealed class FakeCoverArtEnrichmentQueue : ICoverArtEnrichmentQueue
    {
        public List<Guid> EnqueuedArtistIds { get; } = [];

        public void EnqueueArtist(Guid artistId) => EnqueuedArtistIds.Add(artistId);
    }
}
