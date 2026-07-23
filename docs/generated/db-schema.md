# DB schema (generated)

> Regenerate this file by hand after adding/changing a migration — reflect
> `src/PickMeWhatToListen.Infrastructure/Migrations/*_InitialCreate.cs` (and
> any later migrations) here so it stays a readable summary instead of
> making people read raw migration C#.

Current as of migration `20260723193517_InitialCreate`.

## `Artists`

| Column        | SQLite type | Nullable | Notes                                                    |
|---------------|-------------|----------|-----------------------------------------------------------|
| `Id`          | TEXT        | No       | Primary key, GUID                                          |
| `Name`        | TEXT        | No       | Max 200 chars (enforced in `Artist.Create`, not by SQLite) |
| `CreatedAtUtc`| INTEGER     | No       | UTC ticks — see `docs/references/efcore-sqlite-datetimeoffset.md` |
| `IsPicked`    | INTEGER     | No       | Boolean (0/1), indexed (`IX_Artists_IsPicked`)             |
| `PickedAtUtc` | INTEGER     | Yes      | UTC ticks, null until picked                               |

No other tables yet. `docs/product-specs/future-ideas.md` (discography,
tags) will each need their own migration when specced — don't pre-add
columns/tables speculatively.
