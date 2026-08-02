# 0007-artist-metadata-enrichment

Status: Active
Spec: [artist-metadata-enrichment.md](../../product-specs/artist-metadata-enrichment.md)

## Goal

Enrich the artist profile panel with MusicBrainz genres/tags and album+EP discography
with CAA cover thumbnails.

## Scope

- In: MB search/link, genres (count≥5), tags (count≥5, minus genres), release-groups,
  CAA 250px, SQLite cache, profile UI.
- Out: bio, Last.fm, ListenBrainz, user tags, singles.

## Plan

- [x] Product spec
- [x] Domain + migration (Artists metadata, MetadataTerms, ArtistMetadataTerms, ReleaseGroups)
- [x] Application ports + ArtistProfileService
- [x] Infrastructure: MusicBrainz + CAA adapters, EF repository
- [x] Wpf: profile UI (genres, tags, discography)
- [x] Tests + reference docs + db-schema.md
- [x] `dotnet test`

## Decisions & deviations log

- 2026-08-02 — Genres from MB `genres`, tags from MB `tags \ genres`, threshold count≥5
  (not top-N). Wikidata URL stored, content deferred.

## Open items / follow-ups

- ListenBrainz upcoming releases (phase 2)
- Wikipedia bio via stored Wikidata URL
