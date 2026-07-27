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

        var result = await service.AddArtistAsync("Boards of Canada");

        Assert.True(result.Succeeded);
        var all = await repository.GetAllAsync();
        Assert.Single(all);
        Assert.Equal(result.Artist!.Id, all[0].Id);
        Assert.Equal("Boards of Canada", all[0].Name);
    }

    [Theory]
    [InlineData("Мёбиус", "Мебиус")] // ё/е
    [InlineData("Şevval", "Sevval")] // ş/s
    [InlineData("O'Brien", "O’Brien")] // apostrophe variants
    [InlineData("aphex twin", "  APHEX   TWIN  ")] // case + whitespace
    public async Task AddArtistAsync_RejectsNormalizedDuplicate(string existingName, string attemptedName)
    {
        var repository = new FakeArtistRepository().Seed(Artist.Create(existingName));
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        var result = await service.AddArtistAsync(attemptedName);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.DuplicateOf);
        Assert.Equal(existingName, result.DuplicateOf!.Name);
        Assert.Single(await repository.GetAllAsync());
    }

    [Fact]
    public async Task AddArtistAsync_AllowsActuallyDifferentNames()
    {
        var repository = new FakeArtistRepository().Seed(Artist.Create("Autechre"));
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        var result = await service.AddArtistAsync("Aphex Twin");

        Assert.True(result.Succeeded);
        Assert.Equal(2, (await repository.GetAllAsync()).Count);
    }

    [Fact]
    public async Task AddArtistsAsync_SkipsBlankLinesInvalidLinesAndDuplicates()
    {
        var repository = new FakeArtistRepository().Seed(Artist.Create("Autechre"));
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));
        var tooLong = new string('a', Artist.MaxNameLength + 1);

        var result = await service.AddArtistsAsync(
        [
            "Aphex Twin",
            "",
            "   ",
            "AUTECHRE", // duplicate of the seeded artist, case-insensitive
            "Boards of Canada",
            "Boards of Canada", // duplicate within the same batch
            tooLong,
        ]);

        Assert.Equal(2, result.AddedCount); // Aphex Twin, Boards of Canada
        Assert.Equal(2, result.SkippedDuplicateCount); // AUTECHRE, second Boards of Canada
        Assert.Equal(1, result.SkippedInvalidCount); // tooLong
        var all = await repository.GetAllAsync();
        Assert.Equal(3, all.Count); // seeded Autechre + the 2 newly added
    }

    [Fact]
    public async Task GetArtistByIdAsync_ReturnsMatchingArtist()
    {
        var artist = Artist.Create("Aphex Twin");
        var repository = new FakeArtistRepository().Seed(artist);
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        var result = await service.GetArtistByIdAsync(artist.Id);

        Assert.NotNull(result);
        Assert.Equal(artist.Id, result!.Id);
        Assert.Equal("Aphex Twin", result.Name);
    }

    [Fact]
    public async Task GetArtistByIdAsync_ReturnsNull_WhenNoArtistHasThatId()
    {
        var repository = new FakeArtistRepository().Seed(Artist.Create("Aphex Twin"));
        var service = new ArtistCatalogService(repository, new FixedRandomProvider(0));

        var result = await service.GetArtistByIdAsync(Guid.NewGuid());

        Assert.Null(result);
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
