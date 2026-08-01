# 0006-typography-spacing-and-motion

Status: Completed
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
  `ColumnDefinition`s, details-panel transition `Storyboard`),
  `ResponsiveScaleConverter` in `Converters/` registered in
  `Themes/Converters.xaml`, `Themes/Controls.xaml` (`SurfaceCardStyle`
  padding, `PrimaryButtonStyle` responsive chrome + button `FontSize`).
- Out (see `future-ideas.md`): the slot-machine pick-random animation,
  runtime theme switching, `GridSplitter`/resizable panes, body
  typography scaling (`TitleTextStyle`/`CaptionTextStyle`/list rows stay
  fixed — deliberate after visual review).

## Plan

- [x] `Fonts/PT-Root-UI_VF.ttf` + `Fonts/LICENSE.txt`, embedded as a
      `<Resource>` in the `.csproj`.
- [x] `Themes/Typography.xaml` — `AppFontFamily` resource, merged into
      `Controls.xaml`.
- [x] `MainWindow.xaml` — `Window.FontFamily="{StaticResource
      AppFontFamily}"`.
- [x] Spacing: root `Grid` margin `24` → `16`, drop the inter-column
      gap, list panel `BorderThickness="1,1,0,1"`, `SurfaceCardStyle`
      padding `20` → `12`, minor button/textbox padding trims.
- [x] Responsive column split: **1:3** star-sized columns with
      `MinWidth="160"` / `"480"`.
- [x] ~~`ResponsiveFontSizeConverter` + bindings on `Window.FontSize`,
      `TitleTextStyle`, `CaptionTextStyle`.~~ Out of scope — body
      typography stays fixed; only primary-button labels scale.
- [x] `ResponsiveScaleConverter` + bindings on primary control chrome
      (add-artist `TextBox`/`Button`, pick-random `Button` heights/widths).
- [x] Primary-button readability pass: `FontSize` on `PrimaryButtonStyle`
      (base 15, scales with window), taller/wider chrome bases (36/96
      toolbar row, 32 pick-random), `Padding="12,0"` on primary buttons.
- [x] Details panel transition: `Tag`-bound `Binding.TargetUpdated`
      trigger + fade/slide `Storyboard` on the details `Grid`.
      Requires `NotifyOnTargetUpdated=True` on the `Tag` binding — without
      it the event never fires.
- [x] `dotnet build` + `dotnet test` + `dotnet format
      --verify-no-changes` clean; visual check via `dotnet run
      --project src/PickMeWhatToListen.Wpf` + manual review + `xamlmcp`
      (window resize, selection change, transition confirmed).
- [x] Docs: this plan's decisions log, moved to `completed/`.

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
- 2026-08-01 — Spacing: tightened root `Grid` margin (`24`→`16`),
  `SurfaceCardStyle` padding (`20`→`12`), `PrimaryButtonStyle` padding
  (`14,8`→`12,7`), `ThemedTextBoxStyle` padding (`8,6`→`8,5`), and the
  `12`-value inter-row margins in the list column's add-toolbar/list/
  button stack (`12`→`8`, matching the smaller margins already used
  elsewhere in that stack rather than introducing a third spacing value).
  Border merge: dropped the list panel's `Margin="0,0,20,0"` gap entirely
  and overrode its `BorderThickness` locally to `1,1,0,1` (no right
  edge) so it sits directly adjacent to the details panel (unchanged,
  all four sides) — verified via `xamlmcp` (`props` on both panel
  `Border`s: list resolves `1,1,0,1`/`Local`, and the screenshot shows a
  single continuous outer rectangle with one internal divider line, no
  doubled 2px seam). Left the details-panel-internal margins (picked/
  created date lines) untouched — out of this pass's explicit scope.
- 2026-08-01 — **Follow-up iteration on spacing, driven by visual review.**
  After the initial tightening pass above, went further per direct
  feedback while looking at the running app:
  - Root `Grid` margin `16` → `0` (panels now touch the window edge).
  - `SurfaceCardStyle.Padding` `12` → `0` by default, `PrimaryButtonStyle`/
    `ThemedTextBoxStyle` padding → `0` — so the list panel's textbox,
    "Добавить"/"Импорт из файла" toolbar, artist list, and "Выбрать
    случайного" button are all full-bleed within their panel (no inset
    from the border on any side).
  - This over-corrected for the **details panel**, which holds only text
    (name, dates, empty-state placeholder) and read badly glued to the
    border, and for **`LinkButtonStyle`** ("Импорт из файла..."), same
    problem. Fixed by keeping `SurfaceCardStyle.Padding="0"` as the
    default (right for the list panel's full-bleed controls) but adding
    a local `Padding="12"` override on the details panel's `Border` in
    `MainWindow.xaml`, and restoring `LinkButtonStyle.Padding` to `0,4`.
    **Takeaway**: a "make X full-bleed" request doesn't automatically
    apply to every sibling that shares the same style — text-only
    content and interactive full-width controls want different padding,
    so it's a local override on the specific `Border`, not a universal
    style-level change.
  - Zeroing out button/textbox padding also made "Добавить" (52.4×16.4px
    measured via `xamlmcp`), "Выбрать случайного" (299×14.4px), and the
    add-artist `TextBox` (238.6×16.4px) look visually flattened. Fixed
    per explicit sizing request: local `Height="32"` on both the
    `TextBox` and "Добавить" (matches, since both measured ~16.4px
    before — keeps them level in the same row) plus `Width="58"` on
    "Добавить" (~1.1× its prior 52.4px width), and `Height="28"` on
    "Выбрать случайного" (~2× its prior 14.4px, which was already
    marginally shorter than "Добавить" despite sharing the same style —
    likely per-string layout rounding, not worth chasing further).
    Deliberately explicit local `Height`/`Width` rather than a shared
    style change, since the two buttons now have different target sizes
    despite sharing `PrimaryButtonStyle`.
- 2026-08-01 — Responsive column split revised to **1:3** (list:details)
  with `MinWidth="160"` / `"480"` (sums to window `MinWidth="640"`).
- 2026-08-01 — **Control-only responsive sizing (text deferred).** Added
  `ResponsiveScaleConverter`: linear scale from `Window.ActualWidth` against
  reference 760px, clamped between the window min floor (~0.84×) and 1.25×
  so chrome grows with the window but doesn't balloon on ultrawide while
  font sizes stay fixed. Bound on add-artist `TextBox`/`Button` and
  pick-random `Button` in `MainWindow.xaml`. Typography (`Window.FontSize`,
  `TitleTextStyle`, `CaptionTextStyle`) left untouched pending visual review
  — the 0.5 dampening idea applies to text only if/when we add it.
- 2026-08-02 — Details panel transition wired in `MainWindow.xaml`:
  `Tag="{Binding SelectedArtist, NotifyOnTargetUpdated=True}"` on the content
  `Grid`, `Binding.TargetUpdated` → 220ms fade-in + 10px slide-up
  (`QuadraticEase` out, `HandoffBehavior="SnapshotAndReplace"`). First attempt
  omitted `NotifyOnTargetUpdated` — storyboard never started; confirmed working
  after the fix via manual review + xamlmcp selection change.
- 2026-08-01 — Details panel transition is a known simplification: WPF
  updates the bound content before the animation runs, so there's no
  cheap way to show the *actual* old content fading out without a
  snapshot/double-buffer trick. The fade-out phase briefly targets the
  *new* content already faded down, then it fades back in — for the
  short total transition this reads the same as a true crossfade, but is
  worth knowing if it ever looks off during review.

## Open items / follow-ups

- None open for the scope of this plan. Body typography scaling
  (`TitleTextStyle`, list rows, details panel dates) was explicitly
  deferred after review — reopen only via a new spec if needed.
