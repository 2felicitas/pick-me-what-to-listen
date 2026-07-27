# 0004-artist-profile-panel

Status: Completed
Spec: [docs/product-specs/artist-profile-panel.md](../../product-specs/artist-profile-panel.md)

## Goal

Add a side panel to `MainWindow` that shows the selected artist's name,
fetched via a new single-artist read path, with click-to-select and
click-again-or-close-button-to-deselect.

## Scope

- In: `ArtistCatalogService.GetArtistByIdAsync` (+ `Application.Tests`),
  `ArtistProfileViewModel` (Wpf), `MainViewModel.SelectedArtist` +
  `SelectArtistCommand`/`ClosePanelCommand`, `MainWindow.xaml` layout
  (two-column grid, row-as-button, panel `Border`).
- Out (see `artist-profile-panel.md` non-goals): extra fields beyond
  name, editing, visual design polish.

## Plan

- [x] `ArtistCatalogService.GetArtistByIdAsync(Guid)` wrapping
      `IArtistRepository.GetByIdAsync` + `Application.Tests` (found,
      not-found cases, using `FakeArtistRepository`).
- [x] `ArtistProfileViewModel` (Wpf) — immutable display snapshot, same
      pattern as `ArtistRowViewModel`.
- [x] `MainViewModel`: `SelectedArtist` (`[ObservableProperty]`),
      `SelectArtistCommand(Guid id)` (toggles off if re-selecting the
      current artist, otherwise fetches + sets), `ClosePanelCommand()`.
- [x] `MainWindow.xaml`: two-column `Grid` (list/controls column +
      fixed-width panel column spanning all rows), each list row wrapped
      in a flat-styled `Button` bound to `SelectArtistCommand` via
      `RelativeSource AncestorType=ListBox` (no new third-party behaviors
      package needed), panel `Border` with name `TextBlock` + "✕" close
      `Button`. Bumped window `Width`/`MinWidth` to fit the new column.
- [x] Manual smoke test (`dotnet run`) — click a row, click it again,
      click a different row, click "✕". No ViewModel/UI test coverage
      exists yet (see `.cursor/rules/testing.mdc`).
- [x] Docs: flip spec status to Implemented, move this plan to
      `completed/`.

## Decisions & deviations log

- **First pass only made the row's *text* clickable, not the full row
  width, and the ListBox's default selection highlight went out of sync
  with `SelectedArtist`** (stayed highlighted after deselecting via the
  panel's "✕", since it's a separate WPF-internal state we never touch).
  Fixed both together: `ArtistRowContainerStyle` strips `ListBoxItem`'s
  default template entirely (no more built-in highlight to desync), and
  the row `Button`'s custom `ControlTemplate` wraps `ContentPresenter` in
  a `Border Background="{TemplateBinding Background}"` — a bare
  `ContentPresenter` has no fill of its own, so without that `Border`
  only the actual `TextBlock` glyphs (not the empty space around them)
  were hit-test visible, no matter how much `HorizontalAlignment="Stretch"`
  was set on the presenter itself. Recorded as a `.cursor/rules/mvvm-wpf.mdc`
  gotcha so the next custom `ControlTemplate` in this repo doesn't lose a
  day to the same thing.
- **Re-added a row highlight** (hover + selected) after the container
  strip above removed it, per user request during manual testing — driven
  by a `RowIsSelectedConverter` (`IMultiValueConverter` comparing the row's
  `Id` to `MainViewModel.SelectedArtist.Id` via a `MultiBinding` inside a
  `Style.Triggers > DataTrigger`) rather than `ListBoxItem.IsSelected`, to
  keep it consistent with the actual selection source of truth.

## Open items / follow-ups

- None open.
