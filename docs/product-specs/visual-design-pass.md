# Visual design pass: list/details layout

Status: **Implemented** (see `docs/exec-plans/completed/0005-visual-design-pass.md`
and `docs/exec-plans/completed/0006-typography-spacing-and-motion.md`)

## Goal

Give the main window an actual design instead of a functional wireframe:
adopt the [list/details pattern](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/list-details)
as the structural basis of the screen, introduce a warm color palette that
lives in one editable place, and restyle every control with a flat look
(no gradients, no glassmorphism, no shadows).

## Behavior

### Layout

- Side-by-side list/details, two columns:
  - **List column** (left) is a self-contained "management" block,
    bookended top and bottom: an add-artist toolbar at the top, the
    scrollable artist list in the middle, and the "Выбрать случайного"
    button spanning the full column width at the bottom. A status banner
    (duplicate name, import summary, "all picked") sits between the
    toolbar and the list, shown only when `StatusMessage` is set.
  - **Details column** (right, fills remaining width) is the "result"
    pane: it shows whichever artist is currently selected, however that
    selection happened (row click or a random pick).
- The add-artist toolbar keeps the text field + primary "Добавить" button
  on one row; "Импорт из файла..." moves to a secondary, link-styled
  button underneath instead of sitting next to "Добавить" as an equal
  action.
- No `GridSplitter` / user-resizable list column in this pass. The column
  *widths* themselves did move from a fixed pixel value to a proportional
  split that scales with the window — see "Responsive column split" below
  — but that's still not the same as a user dragging a splitter.

### Pick random result surfaces in the details panel, not a banner

- Previously `PickRandomCommand` set a separate `LastPickedArtistName`
  string shown as "Сегодня слушаем: {name}" above the pick button. This is
  removed. Instead, a successful pick now sets `SelectedArtist` to the
  drawn artist — exactly like clicking its row — so the details panel
  becomes the single place that shows "what did I just get". This is a
  direct consequence of adopting list/details: the pattern's whole point
  is that the details pane reflects whatever is selected, and a random
  pick is a selection.
- `StatusMessage` keeps its existing job: errors and informational
  messages that aren't tied to "here's an artist" (duplicate on add,
  import summary, "all artists already picked").

### Details panel content expands

- Beyond the artist's name, the panel now also shows:
  - A "picked" indicator (checkmark + "Отмечен") with the `PickedAtUtc`
    date when `IsPicked` is true.
  - The `CreatedAtUtc` date ("В списке с ...") always.
- This supersedes the "no field beyond name" non-goal in
  `artist-profile-panel.md` — that restriction was a deliberate scoping
  call for the minimal first version, not a permanent one. It's revisited
  here because without it, a random-pick result landing in the panel
  would look identical to just browsing an already-picked artist, which
  defeats the point of moving the result there in the first place.
- Editing artist data from the panel, and anything discography/tag-shaped,
  are still out of scope — unchanged from `artist-profile-panel.md`.

### Empty state

- When nothing is selected, the details panel now shows a short
  placeholder message ("Выберите исполнителя из списка или нажмите
  «Выбрать случайного»") instead of being visually empty. This supersedes
  the earlier "no placeholder text" decision in `artist-profile-panel.md`,
  which was made when the panel was unstyled and any text in an
  unstyled empty box read as a bug rather than an empty state.

### Color palette

- The palette is **not** a runtime-swappable config — no JSON file, no
  reload-without-restart mechanism. The goal is only that changing the
  whole app's look is a one-file edit followed by a rebuild/relaunch, not
  a hunt through every XAML file for hardcoded hex values.
- All raw colors live in `src/PickMeWhatToListen.Wpf/Themes/Colors.xaml`.
  Semantic brushes in `Themes/Brushes.xaml` reference those colors by
  `StaticResource`; every style and every XAML file references only the
  semantic brushes, never a raw color or a named system color
  (`OrangeRed`, `Green`, `Gray`, etc.) directly. See
  `.cursor/rules/theming.mdc` for the enforced convention, including a
  non-obvious WPF gotcha around resources referenced from inside a
  `Style`/`ControlTemplate` defined in a separate merged dictionary file.
- The starting palette is a warm one (terracotta/cream) — see the exec
  plan's decisions log for the exact values chosen. Nothing about the
  layout or bindings depends on these specific colors; swapping the
  palette later is purely an edit to `Colors.xaml`.

### Flat UI

- Solid fills only — no `LinearGradientBrush`/`RadialGradientBrush` on
  backgrounds, no blur/opacity-layered "glass" panels, no drop shadows.
  Surfaces are separated by a thin border color, not elevation/shadow.
- This is an explicit "for now" choice, not a permanent style direction —
  future passes may revisit once the palette itself is settled.

### Typography

- The app gets a defined type family instead of the OS-default font:
  PT Root UI, embedded into the app so the look doesn't depend on the font
  being installed on the user's machine. It's applied once, inherited by
  every control (buttons, text box, list rows, details panel) rather than
  set per-style.
- Font sizes are no longer fixed pixel values: they scale with the
  window's actual width, within a clamped range, so text stays
  comfortably readable both near the window's `MinWidth` floor and on a
  maximized/ultrawide window, without becoming illegibly small or
  absurdly large at either extreme.

### Spacing: borders as the only separator

- Padding and margins across the window are tightened so content fills
  its surface, rather than floating in generous whitespace.
- The list and details columns become directly adjacent — no visible gap
  between them — so the shared border between the two is the only visual
  separator, and the pair reads as one continuous bordered block with a
  single internal dividing line rather than two separate cards.

### Responsive column split

- The list/details split becomes proportional (both columns scale
  together with the window's width) instead of the list column having a
  fixed pixel width and the details column simply absorbing whatever
  space is left. The visual ratio at the app's current default window
  size is preserved, and both panes keep a minimum readable width as the
  window approaches its `MinWidth` floor.

### Details panel transition animation

- Closes the `future-ideas.md` "Slide-out artist panel" idea, but with a
  different, simpler approach than originally sketched there: instead of
  the whole panel sliding in/out of existence, its *content* transitions
  in place whenever the selected artist changes — old content fades out,
  new content fades in while sliding up slightly. This applies uniformly
  to every kind of selection change: an artist getting selected for the
  first time, a selection being cleared back to the empty-state
  placeholder, and switching from one selected artist directly to
  another (row click or a random pick, in any combination).

## Non-goals (for this spec)

- Runtime theme switching / a theme picker UI.
- `GridSplitter` or any other resizable-pane behavior.
- The slot-machine pick-random animation (`future-ideas.md`).
- Any new fields beyond `IsPicked`/`PickedAtUtc`/`CreatedAtUtc` in the
  details panel (discography/tags still wait for their own spec).

## Domain model

No changes. `Artist.IsPicked`, `Artist.PickedAtUtc`, `Artist.CreatedAtUtc`
already exist and are simply surfaced in the UI for the first time.
