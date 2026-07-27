using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
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

    [ObservableProperty]
    private ArtistProfileViewModel? _selectedArtist;

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
            var result = await catalogService.AddArtistAsync(NewArtistName);
            if (!result.Succeeded)
            {
                StatusMessage = $"«{result.DuplicateOf!.Name}» уже есть в списке.";
                return;
            }

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
    private async Task ImportFromFileAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Импорт исполнителей из файла",
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var lines = await File.ReadAllLinesAsync(dialog.FileName);
            var result = await catalogService.AddArtistsAsync(lines);
            var skipped = result.SkippedDuplicateCount + result.SkippedInvalidCount;
            StatusMessage = skipped == 0
                ? $"Добавлено: {result.AddedCount}."
                : $"Добавлено: {result.AddedCount}. Пропущено: {skipped} (уже есть или некорректно).";
            await ReloadArtistsAsync();
        }
        catch (IOException ex)
        {
            StatusMessage = $"Не удалось прочитать файл: {ex.Message}";
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

    [RelayCommand]
    private async Task SelectArtistAsync(Guid id)
    {
        if (SelectedArtist?.Id == id)
        {
            SelectedArtist = null;
            return;
        }

        var artist = await catalogService.GetArtistByIdAsync(id);
        SelectedArtist = artist is null ? null : new ArtistProfileViewModel(artist);
    }

    [RelayCommand]
    private void ClosePanel() => SelectedArtist = null;

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
