using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Wpf.ViewModels;

/// <summary>Read-only display snapshot of the artist shown in the profile panel.</summary>
public sealed class ArtistProfileViewModel(Artist artist)
{
    public Guid Id { get; } = artist.Id;

    public string Name { get; } = artist.Name;
}
