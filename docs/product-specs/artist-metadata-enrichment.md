# Artist metadata enrichment

Status: In progress — implementation landed; move to completed after merge.

## Goal

When an artist is selected, show MusicBrainz-sourced **genres**, **tags** (tags minus
genres), and **discography** (albums and EPs only) with Cover Art Archive thumbnails
in the existing profile panel.

## Data sources (v1)

| Source | Data |
|--------|------|
| MusicBrainz | MBID, `genres`, `tags`, `url-rels` (Wikidata URL only), release-groups (album + EP) |
| Cover Art Archive | Front cover 250px per release-group (earliest release by date) |

Not in v1: Wikipedia/Wikidata content, Last.fm, artist photos, singles/compilations,
ListenBrainz (phase 2).

## Genre / tag rules

- **Genres:** all MusicBrainz `genres[]` entries with `count >= 5`.
- **Tags:** all MusicBrainz `tags[]` entries with `count >= 5`, excluding any whose
  name matches a genre name (case-insensitive, trimmed).
- Sort both lists by `count` descending in the UI.

## Discography rules

- Include release-groups with `primary-type` **Album** or **EP** only (`type=album|ep` browse).
- Exclude rows with any **secondary-type** (Compilation, Live, Soundtrack, …).
- Sort by `first-release-date` descending; keep at most **50** rows in cache/UI.
- Cover Art Archive thumbnails (250px front) are fetched on a **background queue**
  after sync — profile text appears first, covers fill in progressively.
- Missing cover (404 / no artwork): show placeholder tile; list still renders.

## Sync

| Event | Behavior |
|-------|----------|
| First profile open / stale (>30 days) | Refresh from MusicBrainz + CAA |
| MBID unknown | Search by artist name; single hit auto-links; multiple → disambiguation UI |
| Force refresh | Manual «Обновить» (when exposed) or re-select after confirm |
| Rate limit | MusicBrainz: max 1 request/sec; User-Agent: `PickMeWhatToListen/{version} (https://github.com/2felicitas/pick-me-what-to-listen)` |

Offline: show SQLite cache; indicate sync date when stale.

## Schema

See `docs/generated/db-schema.md` after migration. Metadata terms (genres/tags) use
separate tables from future user tagging (`future-ideas.md`).

## UI

Profile panel sections (top to bottom): name + pick dates → **Жанры** (chips) →
**Теги** (chips, hidden if empty) → **Дискография** (48px thumb + year · type · title).
Empty genre section hidden. Loading / not-found / disambiguation states as needed.

## Non-goals

- Biography, Wikidata link in UI, upcoming releases, manual metadata edit.
