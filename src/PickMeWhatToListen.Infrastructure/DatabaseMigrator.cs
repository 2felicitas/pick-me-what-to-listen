using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PickMeWhatToListen.Infrastructure;

/// <summary>
/// Applies pending EF Core migrations. Exposed as a plain <see cref="IServiceProvider"/>-based
/// call so callers outside Infrastructure (e.g. the WPF composition root) never need to
/// reference EF Core types directly.
/// </summary>
public static class DatabaseMigrator
{
    public static async Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var factory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
