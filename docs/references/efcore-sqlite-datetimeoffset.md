# EF Core + SQLite: DateTimeOffset can't be ordered/compared server-side

Package: `Microsoft.EntityFrameworkCore.Sqlite` 10.0.10.

## The gotcha

Querying with `.OrderBy(x => x.SomeDateTimeOffsetProperty)` (or comparing
`DateTimeOffset` values in a `Where`) throws at **query execution time**,
not at migration or build time:

```
System.NotSupportedException: SQLite does not support expressions of type
'DateTimeOffset' in ORDER BY clauses. Convert the values to a supported
type, or use LINQ to Objects to order the results on the client side.
```

This is because SQLite has no native datetime type, and the provider can't
safely translate ordering/comparison over the offset-aware representation
into SQL.

## Fix used in this repo

Store the value as UTC ticks (`long`, native `INTEGER` column) via an EF
Core value conversion, keeping `DateTimeOffset` as the CLR/domain type:

```csharp
builder.Property(a => a.CreatedAtUtc)
    .HasConversion(
        v => v.UtcTicks,
        v => new DateTimeOffset(v, TimeSpan.Zero));

builder.Property(a => a.PickedAtUtc) // nullable
    .HasConversion(
        v => v.HasValue ? v.Value.UtcTicks : (long?)null,
        v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : null);
```

See `src/PickMeWhatToListen.Infrastructure/ArtistConfiguration.cs`. Apply
the same conversion to any new `DateTimeOffset`/`DateTimeOffset?` column
before it's queried with ordering or range comparisons.
