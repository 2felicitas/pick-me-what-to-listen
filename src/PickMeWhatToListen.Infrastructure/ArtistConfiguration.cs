using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Infrastructure;

public sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(Artist.MaxNameLength);

        // SQLite has no native datetime type and can't translate ORDER BY over
        // DateTimeOffset; store as UTC ticks (a plain INTEGER) so sorting/filtering
        // stay server-side instead of falling back to client evaluation.
        builder.Property(a => a.CreatedAtUtc)
            .IsRequired()
            .HasConversion(
                v => v.UtcTicks,
                v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(a => a.PickedAtUtc)
            .HasConversion(
                v => v.HasValue ? v.Value.UtcTicks : (long?)null,
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : null);

        builder.Property(a => a.IsPicked)
            .IsRequired();

        builder.HasIndex(a => a.IsPicked);
    }
}
