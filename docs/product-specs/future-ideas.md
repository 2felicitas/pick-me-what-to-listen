# Future ideas (backlog, not specced)

These are headings only — captured so they're discoverable later without
constraining today's schema or architecture. None of these should be
implemented until they get their own spec file in this folder (see
`index.md`).

## Discography

Track each artist's albums/releases. Likely needs an external metadata
source (e.g. MusicBrainz) — not chosen yet. Will introduce an `Album`
entity related to `Artist`.

## Release tracker

Notify/surface when a followed artist puts out something new. Depends on
Discography existing first. Will need some kind of background sync
(polling an external API) — desktop-app-friendly approach not chosen yet
(could be a scheduled check on app startup rather than a background
service, given this is a simple desktop tool).

## Tagging system

Free-form or curated tags on artists (genre, mood, source of the
recommendation, etc.), likely many-to-many. Could double as a filter for
`PickRandomAsync` (e.g. "pick a random unpicked artist tagged 'electronic'")
— if so, that changes the `ArtistCatalogService.PickRandomAsync` signature,
which should be called out explicitly in whatever spec picks this up.

## Random-pick animation

The "pick random" action should have the slot-machine/roulette-style
animation from an earlier version of this app: a "barrel" of progressively
smaller rows (5–7) showing candidate names spins, decelerates, and settles
on the picked artist landing in the center — similar in feel to
Hearthstone's card-back reveal animation. Purely a `PickMeWhatToListen.Wpf`
concern (no `Domain`/`Application` impact expected) once specced; the part
worth preserving deliberately is the "spin, slow down, land in the center"
feel, not the exact row count or sizing.

## Slide-out artist panel

The artist profile panel (see `artist-profile-panel.md`) is a column on
the right side of the main window (see `visual-design-pass.md` for its
current layout and content). Make it slide out/in on selection instead —
hidden (or collapsed to nothing) when no artist is selected, appearing
(sliding in from the right, presumably with a `Storyboard`/
`ThicknessAnimation` or similar) when a row is clicked, and sliding back
out on deselect. Purely a `PickMeWhatToListen.Wpf` concern.
