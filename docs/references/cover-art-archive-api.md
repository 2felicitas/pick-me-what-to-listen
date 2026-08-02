# Cover Art Archive API (snapshot)

Used by `CoverArtArchiveProvider` for 250px album thumbnails.

## Requirements

- Base URL: `https://coverartarchive.org/`
- User-Agent header recommended (same as MusicBrainz client)
- No API key

## Endpoint used

`GET /release-group/{release_group_mbid}` → JSON with `images[]` (250px thumbnail preferred).

We fetch cover art at **release-group** level — CAA picks a representative release internally.
This avoids an extra MusicBrainz `release?release-group=` browse per album during sync.

## Missing artwork

HTTP 404 or empty `images` → `CoverArtStatus.None`, placeholder tile in UI.

## Policy

Images are user-contributed; use respectfully. See
[Cover Art Archive policy](https://musicbrainz.org/doc/Cover_Art_Archive).
