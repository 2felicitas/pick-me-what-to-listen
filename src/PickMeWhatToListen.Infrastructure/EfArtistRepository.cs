using Microsoft.EntityFrameworkCore;
using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Infrastructure;

/// <summary>
/// Creates a short-lived <see cref="AppDbContext"/> per operation via
/// <see cref="IDbContextFactory{TContext}"/> instead of holding one long-lived
/// context, which is the recommended pattern for desktop apps (WPF has no
/// natural per-request DI scope, and DbContext isn't thread-safe).
/// </summary>
public sealed class EfArtistRepository(IDbContextFactory<AppDbContext> dbContextFactory) : IArtistRepository
{
    public async Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Artists
            .AsNoTracking()
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Artist>> GetUnpickedAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Artists
            .AsNoTracking()
            .Where(a => !a.IsPicked)
            .ToListAsync(cancellationToken);
    }

    public async Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Artists.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Artists.Add(artist);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Artists.Update(artist);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
