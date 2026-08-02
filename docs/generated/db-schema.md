# DB schema (generated)

> Regenerate this file by hand after adding/changing a migration — reflect
> `src/PickMeWhatToListen.Infrastructure/Migrations/*` here so it stays a readable
> summary instead of making people read raw migration C#.

Current as of migration `20260802184212_ArtistMetadataEnrichment`.

## `Artists`

| Column                 | SQLite type | Nullable | Notes                                                    |
|------------------------|-------------|----------|-----------------------------------------------------------|
| `Id`                   | TEXT        | No       | Primary key, GUID                                          |
| `Name`                 | TEXT        | No       | Max 200 chars (enforced in `Artist.Create`, not by SQLite) |
| `CreatedAtUtc`         | INTEGER     | No       | UTC ticks — see `docs/references/efcore-sqlite-datetimeoffset.md` |
| `IsPicked`             | INTEGER     | No       | Boolean (0/1), indexed (`IX_Artists_IsPicked`)             |
| `PickedAtUtc`          | INTEGER     | Yes      | UTC ticks, null until picked                               |
| `MusicBrainzArtistMbid`| TEXT        | Yes      | Unique index (multiple NULL allowed in SQLite)             |
| `WikidataUrl`          | TEXT        | Yes      | From MB url-rels; max 512 chars                            |
| `MetadataSyncStatus`   | TEXT        | No       | `None`, `Ok`, `Ambiguous`, `NotFound`, `Failed`            |
| `MetadataSyncedAtUtc`  | INTEGER     | Yes      | UTC ticks                                                  |
| `MetadataSyncError`    | TEXT        | Yes      | Last sync error message, max 2000 chars                     |

## `MetadataTerms`

| Column        | SQLite type | Nullable | Notes                          |
|---------------|-------------|----------|---------------------------------|
| `Id`          | INTEGER     | No       | PK, autoincrement               |
| `Name`        | TEXT        | No       | Normalized key, unique          |
| `DisplayName` | TEXT        | No       | Original casing from MusicBrainz |

## `ArtistMetadataTerms`

| Column           | SQLite type | Nullable | Notes                                |
|------------------|-------------|----------|---------------------------------------|
| `ArtistId`       | TEXT        | No       | FK → `Artists.Id`                     |
| `MetadataTermId` | INTEGER     | No       | FK → `MetadataTerms.Id`               |
| `Kind`           | TEXT        | No       | `Genre` or `Tag`                      |
| `VoteCount`      | INTEGER     | No       | MusicBrainz `count`                   |

Primary key: (`ArtistId`, `MetadataTermId`, `Kind`).

## `ReleaseGroups`

| Column                        | SQLite type | Nullable | Notes                          |
|-------------------------------|-------------|----------|---------------------------------|
| `Id`                          | TEXT        | No       | PK, GUID                        |
| `ArtistId`                    | TEXT        | No       | FK → `Artists.Id`               |
| `MusicBrainzReleaseGroupMbid` | TEXT        | No       | Unique per artist               |
| `Title`                       | TEXT        | No       | Max 500 chars                   |
| `PrimaryType`                 | TEXT        | No       | `Album` or `EP`                 |
| `FirstReleaseDate`            | TEXT        | Yes      | Partial date string from MB     |
| `CoverReleaseMbid`            | TEXT        | Yes      | Earliest release used for CAA   |
| `CoverArtUrl`                 | TEXT        | Yes      | 250px thumbnail URL             |
| `CoverArtStatus`              | TEXT        | No       | `Unknown`, `Ok`, `None`, `Failed` |

Index: (`ArtistId`, `FirstReleaseDate`).

## Not yet modeled

User-defined tags (`future-ideas.md`) — separate from `MetadataTerms` above.
