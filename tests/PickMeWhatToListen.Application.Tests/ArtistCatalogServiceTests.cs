using PickMeWhatToListen.Application.Tests.TestDoubles;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application.Tests;

public class ArtistCatalogServiceTests
{
    [Fact]
    public async Task AddArtistAsync_AddsArtistToRepository()
    {
        var repository = new FakeArtistRepository();
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        var added = await service.AddArtistAsync("Boards of Canada");

        var all = await repository.GetAllAsync();
        Assert.Single(all);
        Assert.Equal(added.Id, all[0].Id);
        Assert.Equal("Boards of Canada", all[0].Name);
    }

    [Fact]
    public async Task PickRandomAsync_ReturnsNoneLeft_WhenCatalogIsEmpty()
    {
        var repository = new FakeArtistRepository();
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        var result = await service.PickRandomAsync();

        Assert.False(result.Succeeded);
        Assert.Null(result.Artist);
    }

    [Fact]
    public async Task PickRandomAsync_ReturnsNoneLeft_WhenEveryArtistAlreadyPicked()
    {
        var alreadyPicked = Artist.Create("Autechre");
        alreadyPicked.Pick();
        var repository = new FakeArtistRepository().Seed(alreadyPicked);
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        var result = await service.PickRandomAsync();

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task PickRandomAsync_OnlyConsidersUnpickedArtists_AndMarksTheChosenOneAsPicked()
    {
        var alreadyPicked = Artist.Create("Autechre");
        alreadyPicked.Pick();
        var unpicked = Artist.Create("Aphex Twin");
        var repository = new FakeArtistRepository().Seed(alreadyPicked, unpicked);
        var random = new FixedRandomProvider(0);
        var service = new ArtistCatalogService(repository, random);

        var result = await service.PickRandomAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(unpicked.Id, result.Artist!.Id);
        Assert.True(unpicked.IsPicked);
        Assert.NotNull(unpicked.PickedAtUtc);
        Assert.Equal(1, random.LastExclusiveUpperBound); // only one unpicked candidate was in the draw pool
    }

    [Fact]
    public async Task PickRandomAsync_DoesNotMutateAlreadyPickedArtists()
    {
        var alreadyPicked = Artist.Create("Autechre");
        alreadyPicked.Pick();
        var originalPickedAt = alreadyPicked.PickedAtUtc;
        var unpicked = Artist.Create("Aphex Twin");
        var repository = new FakeArtistRepository().Seed(alreadyPicked, unpicked);
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        await service.PickRandomAsync();

        Assert.Equal(originalPickedAt, alreadyPicked.PickedAtUtc);
    }
}
