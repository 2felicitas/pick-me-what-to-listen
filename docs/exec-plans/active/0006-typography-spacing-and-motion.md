# 0006-typography-spacing-and-motion

Status: Active
Spec: [docs/product-specs/visual-design-pass.md](../../product-specs/visual-design-pass.md)

## Goal

Second increment of the visual design pass: give the app a defined
typeface (PT Root UI) with responsive sizing, tighten spacing so the
list/details columns read as one bordered block with a single dividing
line, make the column split proportional instead of fixed-width, and
replace the "slide-out panel" backlog idea with a content
fade/slide-up transition on the details panel whenever the selection
changes.

## Scope

- In: `Fonts/PT-Root-UI_VF.ttf` + `Fonts/LICENSE.txt`,
  `PickMeWhatToListen.Wpf.csproj` resource inclusion, new
  `Themes/Typography.xaml`, `MainWindow.xaml` (`Window.FontFamily`,
  margin/padding tightening, border-only column separation, proportional
  `ColumnDefinition`s, details-panel transition `Storyboard`), a new
  `ResponsiveFontSizeConverter` in `Converters/` registered in
  `Themes/Converters.xaml`, `Themes/Controls.xaml` (`SurfaceCardStyle`
  padding, `PrimaryButtonStyle`/`ThemedTextBoxStyle` padding, responsive
  `FontSize` bindings on `TitleTextStyle`/`CaptionTextStyle`).
- Out (see `future-ideas.md`): the slot-machine pick-random animation,
  runtime theme switching, `GridSplitter`/resizable panes.

## Plan

- [x] `Fonts/PT-Root-UI_VF.ttf` + `Fonts/LICENSE.txt`, embedded as a
      `<Resource>` in the `.csproj`.
- [x] `Themes/Typography.xaml` — `AppFontFamily` resource, merged into
      `Controls.xaml`.
- [x] `MainWindow.xaml` — `Window.FontFamily="{StaticResource
      AppFontFamily}"`.
- [ ] Spacing: root `Grid` margin `24` → `16`, drop the inter-column
      gap, list panel `BorderThickness="1,1,0,1"`, `SurfaceCardStyle`
      padding `20` → `12`, minor button/textbox padding trims.
- [ ] Responsive column split: both `ColumnDefinition`s star-sized at
      today's pixel ratio, with `MinWidth` safety values.
- [ ] `ResponsiveFontSizeConverter` + bindings on `Window.FontSize`,
      `TitleTextStyle`, `CaptionTextStyle`.
- [ ] Details panel transition: `Tag`-bound `Binding.TargetUpdated`
      trigger + fade/slide `Storyboard` on the details `Grid`.
- [ ] `dotnet build` + `dotnet test` + `dotnet format
      --verify-no-changes` clean; visual check via `dotnet run
      --project src/PickMeWhatToListen.Wpf` + `xamlmcp` (screenshots at
      a couple of window sizes, `action`-driven selection change to
      confirm the transition fires).
- [ ] Docs: this plan's decisions log, move to `completed/` once merged.

## Decisions & deviations log

- 2026-08-01 — Font: embedding the variable font file
  (`Figtree[wght].ttf`, one file covers weight 300-900) rather than
  separate static per-weight files, since modern WPF (DirectWrite-backed,
  .NET 10) resolves `FontWeight` against a variable font's `wght` axis —
  covers today's two used weights (`Normal`, `SemiBold`) from a single
  file. Verified via `xamlmcp`: `FontWeight="SemiBold"` on the "Добавить"
  button resolved correctly against the variable font's weight axis with
  no fallback needed — the flagged static-file fallback wasn't required.
- 2026-08-01 — **Deviation: Figtree replaced with PT Root UI.** After
  wiring Figtree in and applying it, the app's Cyrillic text (the bulk of
  the UI — it's all Russian) still rendered in the OS-default fallback
  font, not Figtree. Figtree's Cyrillic support exists only as an
  unmerged GitHub PR against `erikdkennedy/figtree` — no released version
  (including the `Figtree[wght].ttf` embedded here) actually contains
  Cyrillic glyphs, so DirectWrite was silently falling back per-glyph-run
  for every Cyrillic character. Switched to **PT Root UI** (Paratype,
  OFL-1.1, `github.com/font-archive/PT-Root-UI`): also a variable font
  (`Fonts/PT-Root-UI_VF.ttf`, `wght` axis 300-700, named instances
  Light/Regular/Medium/Bold), internal family name `PT Root UI VF`
  (registered in the font's own `name` table — matters because the pack
  URI's `#`-suffix must match this exactly, not the file name), and
  verified via `fontTools` to have full Cyrillic coverage (А-Я, а-я, Ё/ё
  all present in `cmap`). License file swapped from `Fonts/OFL.txt` to
  `Fonts/LICENSE.txt` (same OFL-1.1 text, PT Root UI's own copyright
  header/Reserved Font Name block from Paratype) — pulled the official
  copy from the font-archive mirror rather than trusting a redistribution
  missing the required copyright header. `AppFontFamily` in
  `Typography.xaml` and the `<Resource>`/`<None>` includes in the `.csproj`
  updated accordingly; `Figtree[wght].ttf`/`Fonts/OFL.txt` deleted.
  **Gotcha for future font swaps**: always verify Cyrillic (or whatever
  non-Latin script the UI needs) coverage via `fontTools`'
  `font.getBestCmap()` *before* wiring a font in, not after a visual
  check catches the fallback — a variable font's weight axis resolving
  correctly says nothing about its character coverage.
- 2026-08-01 — Details panel transition is a known simplification: WPF
  updates the bound content before the animation runs, so there's no
  cheap way to show the *actual* old content fading out without a
  snapshot/double-buffer trick. The fade-out phase briefly targets the
  *new* content already faded down, then it fades back in — for the
  short total transition this reads the same as a true crossfade, but is
  worth knowing if it ever looks off during review.

## Open items / follow-ups

None yet — update as work proceeds.
