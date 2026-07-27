# Artist profile panel

Status: Implemented (see `docs/exec-plans/completed/0004-artist-profile-panel.md`)

## Goal

Clicking an artist in the catalog list shows its details in a panel docked
to the side of the main window. Today that's just the name, but the panel
is the anchor point for surfacing discography/tags/etc. as those get
specced later, without a redesign each time.

## Behavior

- The panel is a permanently visible column on the right side of the main
  window — a plain bordered "plate" — not something that appears/disappears
  or resizes the window on selection. When nothing is selected it's just
  empty (no placeholder text); visual styling (background/border color) is
  deferred to the "Visual design pass" future idea.
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
  panel). A "✕" button inside the panel does the same thing. Both are
  equivalent, redundant affordances for the same action.
- Selecting a different row while one is already selected simply replaces
  the panel's content — no need to deselect first.

## Non-goals (for this spec)

- Any field beyond the artist's name (no `CreatedAtUtc`/`IsPicked`/
  `PickedAtUtc` in the panel yet, even though `Artist` already carries
  them) — deferred until discography/tags actually land, per the product
  decision to keep this pass minimal.
- Editing artist data from the panel.
- Visual design (colors, spacing, "secondary" panel styling) — see
  `future-ideas.md` → "Visual design pass".

## Domain model

No schema or `Artist` changes. Uses the existing
`IArtistRepository.GetByIdAsync(Guid)` (already implemented in
`EfArtistRepository`/`FakeArtistRepository`, previously unused) via a new
thin `ArtistCatalogService.GetArtistByIdAsync` wrapper.
