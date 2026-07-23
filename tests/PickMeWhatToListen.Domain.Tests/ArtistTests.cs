using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Domain.Tests;

public class ArtistTests
{
    [Fact]
    public void Create_TrimsNameAndSetsDefaults()
    {
        var artist = Artist.Create("  Boards of Canada  ");

        Assert.Equal("Boards of Canada", artist.Name);
        Assert.NotEqual(Guid.Empty, artist.Id);
        Assert.False(artist.IsPicked);
        Assert.Null(artist.PickedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ThrowsOnEmptyName(string? name)
    {
        Assert.Throws<ArgumentException>(() => Artist.Create(name!));
    }

    [Fact]
    public void Create_ThrowsWhenNameTooLong()
    {
        var tooLong = new string('a', Artist.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(() => Artist.Create(tooLong));
    }

    [Fact]
    public void Pick_SetsIsPickedAndTimestamp()
    {
        var artist = Artist.Create("Autechre");
        var pickedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        artist.Pick(pickedAt);

        Assert.True(artist.IsPicked);
        Assert.Equal(pickedAt, artist.PickedAtUtc);
    }

    [Fact]
    public void Pick_ThrowsWhenAlreadyPicked()
    {
        var artist = Artist.Create("Aphex Twin");
        artist.Pick();

        Assert.Throws<InvalidOperationException>(() => artist.Pick());
    }
}
