using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PickMeWhatToListen.Application;

namespace PickMeWhatToListen.Wpf.ViewModels;

public sealed partial class MainViewModel(ArtistCatalogService catalogService) : ObservableObject
{
    public ObservableCollection<ArtistRowViewModel> Artists { get; } = [];

    [ObservableProperty]
    private string _newArtistName = string.Empty;

    [ObservableProperty]
    private string? _lastPickedArtistName;

    [ObservableProperty]
    private string? _statusMessage;

    public async Task InitializeAsync() => await ReloadArtistsAsync();

    [RelayCommand]
    private async Task AddArtistAsync()
    {
        if (string.IsNullOrWhiteSpace(NewArtistName))
        {
            return;
        }

        try
        {
            await catalogService.AddArtistAsync(NewArtistName);
            NewArtistName = string.Empty;
            StatusMessage = null;
            await ReloadArtistsAsync();
        }
        catch (ArgumentException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task PickRandomAsync()
    {
        var result = await catalogService.PickRandomAsync();
        if (!result.Succeeded)
        {
            LastPickedArtistName = null;
            StatusMessage = "Все исполнители уже отмечены как прослушанные — добавьте новых.";
            return;
        }

        LastPickedArtistName = result.Artist!.Name;
        StatusMessage = null;
        await ReloadArtistsAsync();
    }

    private async Task ReloadArtistsAsync()
    {
        var artists = await catalogService.GetAllArtistsAsync();

        Artists.Clear();
        foreach (var artist in artists)
        {
            Artists.Add(new ArtistRowViewModel(artist));
        }
    }
}
