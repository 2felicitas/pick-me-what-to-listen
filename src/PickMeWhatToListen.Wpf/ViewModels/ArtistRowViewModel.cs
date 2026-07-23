using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Wpf.ViewModels;

/// <summary>Read-only display snapshot of an <see cref="Artist"/> for the catalog list.</summary>
public sealed class ArtistRowViewModel(Artist artist)
{
    public Guid Id { get; } = artist.Id;

    public string Name { get; } = artist.Name;

    public bool IsPicked { get; } = artist.IsPicked;
}
