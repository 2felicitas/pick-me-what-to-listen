# 0005-visual-design-pass

Status: Completed
Spec: [docs/product-specs/visual-design-pass.md](../../product-specs/visual-design-pass.md)

## Goal

Rebuild the main window on the list/details pattern, introduce a
one-file-editable warm color palette, and restyle every control flat
(no gradients, no glassmorphism, no shadows) — replacing the current
unstyled wireframe.

## Scope

- In: `MainWindow.xaml` relayout, `Themes/Colors.xaml` +
  `Themes/Brushes.xaml` + `Themes/Converters.xaml` + `Themes/Controls.xaml`,
  `App.xaml` merge, `MainViewModel`/`ArtistProfileViewModel` changes needed
  to feed the new details panel content and to route a random pick through
  `SelectedArtist` instead of a separate banner string, a new
  `.cursor/rules/theming.mdc`.
- Out (see `future-ideas.md`): `GridSplitter`/resizable list column,
  slide-out panel animation, slot-machine pick-random animation, runtime
  theme switching.

## Plan

- [x] Spec: `docs/product-specs/visual-design-pass.md` + index/non-goals
      updates in `core-loop.md` and `artist-profile-panel.md`.
- [x] This exec plan.
- [x] `.cursor/rules/theming.mdc` — colors-in-one-file convention.
- [x] `Themes/Colors.xaml` — raw warm palette `Color` resources.
- [x] `Themes/Brushes.xaml` — semantic `SolidColorBrush` resources over
      the colors above.
- [x] `Themes/Converters.xaml` — moved converters out of `App.xaml` (not
      in the original plan; needed once `ArtistRowButtonStyle` moved into
      `Controls.xaml` — see decisions log).
- [x] `Themes/Controls.xaml` — `PrimaryButtonStyle`, `LinkButtonStyle`,
      `TitleTextStyle`, `CaptionTextStyle`, `SurfaceCardStyle`, plus the
      restyled `ArtistRowButtonStyle`/`ArtistRowContainerStyle` moved out
      of `MainWindow.xaml`.
- [x] `App.xaml` — merge the four theme dictionaries.
- [x] `ArtistProfileViewModel` — add `IsPicked`, `PickedAtUtc`,
      `CreatedAtUtc`.
- [x] `MainViewModel` — drop `LastPickedArtistName`; `PickRandomAsync`
      success sets `SelectedArtist` instead.
- [x] `MainWindow.xaml` — list/details relayout per the spec.
- [x] `dotnet build` + `dotnet test` clean; visual check via
      `dotnet run --project src/PickMeWhatToListen.Wpf` (and `xamlmcp` if
      useful).

## Decisions & deviations log

- 2026-08-01 — Palette: warm terracotta/cream v1. Window background
  `#FBF3EA`, surfaces `#FFFFFF`, border `#E8DCCC`, text primary `#3B2A20`,
  text secondary `#8A7566`, accent `#D97A3F` (hover `#C4692F`, pressed
  `#AD5A26`), accent contrast `#FFFFFF`, accent-soft row selection
  `#F3DCC4`, row hover `#F3E9DD`, success (picked checkmark) `#7C8A4C`
  (warm olive, not green), error/status `#B3462B` (warm rust, not
  `OrangeRed`). Lives entirely in `Colors.xaml` so it's a one-file edit
  to change later.
- 2026-08-01 — Pick-random result now selects into the details panel
  (`SelectedArtist`) instead of a separate `LastPickedArtistName` banner —
  a direct consequence of the list/details pattern's own semantics. See
  spec for the full reasoning.
- 2026-08-01 — Details panel gains `IsPicked`/`PickedAtUtc`/`CreatedAtUtc`
  display, reversing the earlier "name only" non-goal from
  `artist-profile-panel.md` — needed so a random-pick result reads as
  distinct from just browsing an already-picked artist.
- 2026-08-01 — Deviation from the plan: added `Themes/Converters.xaml`
  (not originally listed). `dotnet build`/`dotnet test` passed but
  `dotnet run` threw a `XamlParseException` (`RowIsSelectedConverter` not
  found) the first time the `ListBox` measured a row, because
  `ArtistRowButtonStyle` (now in `Controls.xaml`) referenced a converter
  declared only in `App.xaml`'s own `Application.Resources` — outside
  `Controls.xaml`'s own merge chain. A `StaticResource` inside a
  `Style`/`ControlTemplate`'s deferred content, when that style lives in a
  separately merged dictionary file, only resolves against that file's
  own `MergedDictionaries`, not the full `Application.Resources` tree it
  ends up part of. Fixed by moving all three converters into
  `Themes/Converters.xaml` and merging it into `Controls.xaml`. Recorded
  in `.cursor/rules/theming.mdc` since it's non-obvious and will bite
  again if a future style references an App-level-only resource.
- 2026-08-01 — Deviation: added `Language="ru-RU"` to `MainWindow`.
  Verified via `xamlmcp` screenshots that the details panel's date
  `StringFormat` bindings (`PickedAtUtc`/`CreatedAtUtc`) rendered English
  month names ("1 August 2026") by default, since WPF's `FrameworkElement.
  Language` defaults to `en-US` regardless of OS locale. The rest of the
  UI is Russian text, so an English date read as a bug, not a locale
  choice. Fixed by setting `Language="ru-RU"` on the `Window`, confirmed
  Russian month names ("1 августа 2026") afterward.
- 2026-08-01 — Verified end-to-end via `xamlmcp` (`dotnet run`, attach,
  `search`/`action`/`screenshot`): add toolbar, list with warm palette,
  row selection highlight, empty-state placeholder, pick-random landing in
  the details panel with picked/created dates, and re-click deselect all
  behave as specced.

## Open items / follow-ups

- None open for the scope of this plan (list/details layout, warm
  palette, flat styling). Typography (Figtree, responsive font sizing),
  tighter border-only spacing, the proportional responsive column split,
  and the details-panel transition animation are a second increment of
  the same spec — see
  [0006-typography-spacing-and-motion.md](../active/0006-typography-spacing-and-motion.md).
