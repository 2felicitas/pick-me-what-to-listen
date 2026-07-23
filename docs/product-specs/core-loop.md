# Core loop: add artist, pick random

Status: **Implemented** (see `docs/exec-plans/active/0001-bootstrap-repo-and-core-loop.md` for the build log).

## Goal

Keep a catalog of artists the user wants to listen to, and get a random
suggestion from the ones not yet listened to.

## Behavior

- **Add artist**: user enters a name and adds it to the catalog. Name is
  required, trimmed, max 200 characters. Duplicate names are currently
  allowed (no uniqueness constraint) — revisit if this becomes annoying in
  practice.
- **Pick random**: draws a random artist from those with `IsPicked == false`
  and marks it `IsPicked = true` permanently (with a `PickedAtUtc`
  timestamp). If there are no unpicked artists left, the UI shows a message
  instead of picking anything — this is an expected state, not an error.
- `IsPicked` is **not** a "currently selected" flag — it never gets unset by
  picking a different artist. It's a durable "have I already picked/listened
  to this one" marker per artist. The UI distinguishes picked vs. unpicked
  rows in the list (checkmark + strikethrough) and separately shows a
  "just picked" result banner for the latest draw.

## Non-goals (for this spec)

- Editing or deleting artists.
- Un-marking a picked artist (no "put it back in the pool" action yet).
- Anything about discography, albums, releases, or tags — see
  `future-ideas.md`.

## Domain model

See `ARCHITECTURE.md` → "Persistence model" for the current `Artists` table
shape, and `src/PickMeWhatToListen.Domain/Artist.cs` for the invariants.
