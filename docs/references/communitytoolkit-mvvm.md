# CommunityToolkit.Mvvm

Package: `CommunityToolkit.Mvvm` (installed: 8.4.2). Source-generator-based
MVVM helpers, part of the .NET Community Toolkit.

## Pattern used in this repo

```csharp
public sealed partial class MainViewModel(ArtistCatalogService catalogService) : ObservableObject
{
    [ObservableProperty]
    private string _newArtistName = string.Empty;

    [RelayCommand]
    private async Task AddArtistAsync() { /* ... */ }
}
```

- `[ObservableProperty]` on a `private` field generates a public
  PascalCase property (`_newArtistName` → `NewArtistName`) with
  `INotifyPropertyChanged` plumbing. The class must be `partial`.
- `[RelayCommand]` on a method generates an `ICommand` property named
  `<MethodName>Command` (e.g. `AddArtistAsync` → `AddArtistCommand`),
  usable directly from XAML `Command="{Binding AddArtistCommand}"`.
- Works with async `Task`-returning methods out of the box (no manual
  `AsyncRelayCommand` wiring needed).

See `src/PickMeWhatToListen.Wpf/ViewModels/MainViewModel.cs` for the full
implementation.
