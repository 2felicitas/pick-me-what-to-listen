# Artist profile panel

Status: Implemented (see `docs/exec-plans/completed/0004-artist-profile-panel.md`).
Content and empty-state behavior were revised by
`docs/product-specs/visual-design-pass.md` — see that spec for what
changed and why.

## Goal

Clicking an artist in the catalog list shows its details in a panel docked
to the side of the main window. Today that's just the name, but the panel
is the anchor point for surfacing discography/tags/etc. as those get
specced later, without a redesign each time.

## Behavior

- The panel is a permanently visible column on the right side of the main
  window — not something that appears/disappears or resizes the window on
  selection (see `future-ideas.md` → "Slide-out artist panel" for a future
  revisit of that). When nothing is selected it shows a short placeholder
  message rather than being visually empty — see `visual-design-pass.md`
  for the exact copy and rationale (this supersedes the original "no
  placeholder text" call, made back when the panel was unstyled).
- Clicking an artist row selects it and loads its details via a dedicated
  single-artist read path (`ArtistCatalogService.GetArtistByIdAsync`,
  backed by the already-existing but previously-unused
  `IArtistRepository.GetByIdAsync`) rather than reusing the row data
  already held in memory for the list. This is deliberate even though
  today the panel only shows the name (which the list row already has):
  once discography/tags exist, that data won't be preloaded into the list
  query, so the panel needs its own fetch — building that path now avoids
  reworking the selection flow later.
- Clicking the currently-selected row again deselects it (clears the
  panel). There's no separate close button in the panel itself — an "✕"
  there read as "close the window/app" rather than "clear the selection",
  which was confusing; re-clicking the row is the one, unambiguous way to
  deselect.
- Selecting a different row while one is already selected simply replaces
  the panel's content — no need to deselect first.

## Non-goals (for this spec)

- Editing artist data from the panel.
- Anything discography/tag-shaped — still waits for its own spec.

`CreatedAtUtc`/`IsPicked`/`PickedAtUtc` **are** now shown in the panel —
see `visual-design-pass.md`. The original "name only" scoping call here
was deliberate for the minimal first version, not permanent; it was
revisited once the panel started doubling as the pick-random result
surface, where showing only a name made a fresh pick indistinguishable
from browsing an already-picked artist.

## Domain model

No schema or `Artist` changes. Uses the existing
`IArtistRepository.GetByIdAsync(Guid)` (already implemented in
`EfArtistRepository`/`FakeArtistRepository`, previously unused) via a new
thin `ArtistCatalogService.GetArtistByIdAsync` wrapper.
