using Microsoft.EntityFrameworkCore;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Artist> Artists => Set<Artist>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
