# EF Core: IDbContextFactory for apps without a per-request DI scope

Package: `Microsoft.EntityFrameworkCore` (via `Microsoft.EntityFrameworkCore.Sqlite`) 10.0.10.

## Why

`DbContext` is not thread-safe and is meant to represent one short unit of
work. ASP.NET Core gets this for free via a per-HTTP-request DI scope +
`AddDbContext` (scoped lifetime). A WPF app has no equivalent natural scope
boundary — the whole app is effectively one long-lived "request" — so
registering `AppDbContext` as scoped/singleton and injecting it once leads
to a single long-lived instance being reused (and potentially accessed)
across unrelated operations.

## Pattern used in this repo

```csharp
services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));
```

```csharp
public sealed class EfArtistRepository(IDbContextFactory<AppDbContext> dbContextFactory) : IArtistRepository
{
    public async Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken ct = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        return await dbContext.Artists.AsNoTracking().OrderBy(a => a.CreatedAtUtc).ToListAsync(ct);
    }
}
```

`IDbContextFactory<T>` itself is safe to register once (transient context
creation is thread-safe); each repository method creates and disposes its
own short-lived `AppDbContext`. Contexts obtained this way are **not**
tracked by the DI container and must be disposed explicitly (`await using`).

Design-time tooling (`dotnet ef migrations add`) needs its own separate
factory implementing `IDesignTimeDbContextFactory<AppDbContext>` — see
`AppDbContextFactory.cs`. That's a different interface from
`IDbContextFactory<T>` above and serves a different purpose (letting the EF
CLI construct the context without booting the WPF host).
