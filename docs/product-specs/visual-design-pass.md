# Visual design pass: list/details layout

Status: In progress (see `docs/exec-plans/active/0005-visual-design-pass.md`)

## Goal

Give the main window an actual design instead of a functional wireframe:
adopt the [list/details pattern](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/list-details)
as the structural basis of the screen, introduce a warm color palette that
lives in one editable place, and restyle every control with a flat look
(no gradients, no glassmorphism, no shadows).

## Behavior

### Layout

- Side-by-side list/details, two columns:
  - **List column** (left, fixed width ~300px) is a self-contained
    "management" block, bookended top and bottom: an add-artist toolbar at
    the top, the scrollable artist list in the middle, and the "Выбрать
    случайного" button spanning the full column width at the bottom. A
    status banner (duplicate name, import summary, "all picked") sits
    between the toolbar and the list, shown only when `StatusMessage` is
    set.
  - **Details column** (right, fills remaining width) is the "result"
    pane: it shows whichever artist is currently selected, however that
    selection happened (row click or a random pick).
- The add-artist toolbar keeps the text field + primary "Добавить" button
  on one row; "Импорт из файла..." moves to a secondary, link-styled
  button underneath instead of sitting next to "Добавить" as an equal
  action.
- No `GridSplitter` / resizable list column in this pass — fixed width is
  good enough until proven otherwise.

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

## Non-goals (for this spec)

- Runtime theme switching / a theme picker UI.
- `GridSplitter` or any other resizable-pane behavior.
- The slide-out details panel animation (`future-ideas.md`).
- The slot-machine pick-random animation (`future-ideas.md`).
- Any new fields beyond `IsPicked`/`PickedAtUtc`/`CreatedAtUtc` in the
  details panel (discography/tags still wait for their own spec).

## Domain model

No changes. `Artist.IsPicked`, `Artist.PickedAtUtc`, `Artist.CreatedAtUtc`
already exist and are simply surfaced in the UI for the first time.
