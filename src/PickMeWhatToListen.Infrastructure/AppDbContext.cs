using Microsoft.EntityFrameworkCore;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Artist> Artists => Set<Artist>();

    public DbSet<MetadataTerm> MetadataTerms => Set<MetadataTerm>();

    public DbSet<ArtistMetadataTerm> ArtistMetadataTerms => Set<ArtistMetadataTerm>();

    public DbSet<ReleaseGroup> ReleaseGroups => Set<ReleaseGroup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
