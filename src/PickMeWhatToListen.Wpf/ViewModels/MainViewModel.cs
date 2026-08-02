using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PickMeWhatToListen.Application;
using PickMeWhatToListen.Application.Abstractions;

namespace PickMeWhatToListen.Wpf.ViewModels;

public sealed partial class MainViewModel(
    ArtistCatalogService catalogService,
    ArtistProfileService profileService,
    ICoverArtEnrichmentNotifier coverArtEnrichmentNotifier) : ObservableObject
{
    private CancellationTokenSource? _profileLoadCts;
    private bool _coverArtNotifierRegistered;

    public ObservableCollection<ArtistRowViewModel> Artists { get; } = [];

    [ObservableProperty]
    private string _newArtistName = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private ArtistProfileViewModel? _selectedArtist;

    /// <summary>
    /// Drives the profile-panel enter animation; updated only on artist switches,
    /// not when background cover-art enrichment refreshes the same profile.
    /// </summary>
    [ObservableProperty]
    private object? _profilePanelTransitionKey;

    public async Task InitializeAsync()
    {
        if (!_coverArtNotifierRegistered)
        {
            coverArtEnrichmentNotifier.Register(OnArtistCoverArtUpdated);
            _coverArtNotifierRegistered = true;
        }

        await ReloadArtistsAsync();
    }

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
            StatusMessage = "Все исполнители уже отмечены как прослушанные — добавьте новых.";
            return;
        }

        // The result surfaces in the details panel (like clicking its row would),
        // not a separate banner — see docs/product-specs/visual-design-pass.md.
        StatusMessage = null;
        await ReloadArtistsAsync();
        await LoadProfileAsync(result.Artist!.Id);
    }

    [RelayCommand]
    private async Task ConfirmMusicBrainzMatchAsync(MusicBrainzCandidateViewModel candidate)
    {
        var token = BeginProfileLoad();
        var artist = (await catalogService.GetArtistByIdAsync(candidate.ArtistId))!;
        SetSelectedArtist(ArtistProfileViewModel.Loading(artist));

        try
        {
            var result = await profileService.ConfirmMusicBrainzMatchAsync(candidate.ArtistId, candidate.Mbid, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (!result.Found || result.Profile is null)
            {
                SetSelectedArtist(null);
                return;
            }

            SetSelectedArtist(ArtistProfileViewModel.FromResult(result));
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
            {
                SetSelectedArtist(new ArtistProfileViewModel(artist, [], [], [], [], isLoading: false, statusMessage: ex.Message));
            }
        }
    }

    [RelayCommand]
    private async Task RefreshSelectedProfileAsync()
    {
        if (SelectedArtist is null)
        {
            return;
        }

        var artistId = SelectedArtist.Id;
        var token = BeginProfileLoad();
        var artist = await catalogService.GetArtistByIdAsync(artistId);
        if (artist is null)
        {
            SetSelectedArtist(null);
            return;
        }

        SetSelectedArtist(ArtistProfileViewModel.Loading(artist));

        try
        {
            var result = await profileService.ForceRefreshAsync(artistId, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            SetSelectedArtist(result.Found && result.Profile is not null
                ? ArtistProfileViewModel.FromResult(result)
                : null);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
            {
                SetSelectedArtist(new ArtistProfileViewModel(artist, [], [], [], [], isLoading: false, statusMessage: ex.Message));
            }
        }
    }

    [RelayCommand]
    private async Task SelectArtistAsync(Guid id)
    {
        if (SelectedArtist?.Id == id)
        {
            SetSelectedArtist(null);
            return;
        }

        var artist = await catalogService.GetArtistByIdAsync(id);
        if (artist is null)
        {
            SetSelectedArtist(null);
            return;
        }

        await LoadProfileAsync(id);
    }

    private async Task LoadProfileAsync(Guid artistId)
    {
        var token = BeginProfileLoad();

        try
        {
            var artist = await catalogService.GetArtistByIdAsync(artistId);
            if (artist is null)
            {
                SetSelectedArtist(null);
                return;
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            SetSelectedArtist(ArtistProfileViewModel.Loading(artist));
            var result = await profileService.GetProfileAsync(artistId, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (!result.Found || result.Profile is null)
            {
                SetSelectedArtist(null);
                return;
            }

            SetSelectedArtist(ArtistProfileViewModel.FromResult(result));
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var artist = await catalogService.GetArtistByIdAsync(artistId);
            SetSelectedArtist(artist is null
                ? null
                : new ArtistProfileViewModel(artist, [], [], [], [], isLoading: false, statusMessage: ex.Message));
        }
    }

    private void SetSelectedArtist(ArtistProfileViewModel? profile)
    {
        var previousId = SelectedArtist?.Id;
        SelectedArtist = profile;

        if (previousId != profile?.Id)
        {
            ProfilePanelTransitionKey = profile?.Id ?? Guid.Empty;
        }
    }

    private CancellationToken BeginProfileLoad()
    {
        _profileLoadCts?.Cancel();
        _profileLoadCts?.Dispose();
        _profileLoadCts = new CancellationTokenSource();
        return _profileLoadCts.Token;
    }

    private void OnArtistCoverArtUpdated(Guid artistId)
    {
        if (SelectedArtist?.Id != artistId)
        {
            return;
        }

        _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(RefreshSelectedArtistFromCacheAsync);
    }

    private async Task RefreshSelectedArtistFromCacheAsync()
    {
        var artistId = SelectedArtist?.Id;
        if (artistId is null)
        {
            return;
        }

        try
        {
            var result = await profileService.GetCachedProfileAsync(artistId.Value);
            if (result?.Found != true || result.Profile is null || SelectedArtist?.Id != artistId)
            {
                return;
            }

            SelectedArtist = ArtistProfileViewModel.FromResult(result);
        }
        catch
        {
            // Background refresh — ignore.
        }
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
