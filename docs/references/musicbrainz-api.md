# MusicBrainz API (snapshot)

Used by `MusicBrainzMetadataProvider` for artist metadata enrichment.

## Requirements

- Base URL: `https://musicbrainz.org/ws/2/`
- **User-Agent** header required (app name + contact URL)
- **Rate limit: 1 request per second** per IP — enforced via `MusicBrainzRateLimiter`
- JSON: `fmt=json` query parameter
- **Do not combine `HttpClient.BaseAddress` with relative paths like `artist?…`** — .NET drops
  the trailing `2/` segment and requests `/ws/artist` instead of `/ws/2/artist`. Our client builds
  absolute URIs from `MusicBrainzMetadataProvider.ApiRoot` instead.

## Endpoints used

| Call | URL pattern |
|------|-------------|
| Search artist | `GET artist?query=artist:"{name}"&fmt=json&limit=5` |
| Artist metadata | `GET artist/{mbid}?inc=genres+tags+url-rels&fmt=json` |
| Release groups | `GET release-group?artist={mbid}&type=album|ep&fmt=json` |
| Releases in group | `GET release?release-group={rg_mbid}&fmt=json&limit=100&offset=N` |

## Genres vs tags

Artist lookup returns separate arrays:

- `genres[]` — `{ name, count }` → stored/displayed as **Жанры**
- `tags[]` — `{ name, count }` → **Теги** after removing names present in genres

Both filtered to `count >= 5` in `ArtistMetadataRules.BuildTerms`.

## Wikidata URL

From `relations[]` where `type == "wikidata"`, field `url.resource`. Stored on
`Artist.WikidataUrl`; content not fetched in v1.

## Pagination

Browse endpoints return max 100 items; increment `offset` until a page returns
fewer than `limit` results.

## License

Non-commercial API use is free (MetaBrainz). See MusicBrainz commercial plans for
other use cases.
