using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Infrastructure;

public sealed class ReleaseGroupConfiguration : IEntityTypeConfiguration<ReleaseGroup>
{
    public void Configure(EntityTypeBuilder<ReleaseGroup> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.MusicBrainzReleaseGroupMbid)
            .IsRequired()
            .HasMaxLength(ReleaseGroup.MaxMbidLength);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(ReleaseGroup.MaxTitleLength);

        builder.Property(r => r.PrimaryType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.FirstReleaseDate)
            .HasMaxLength(ReleaseGroup.MaxDateLength);

        builder.Property(r => r.CoverReleaseMbid)
            .HasMaxLength(ReleaseGroup.MaxMbidLength);

        builder.Property(r => r.CoverArtUrl)
            .HasMaxLength(ReleaseGroup.MaxCoverUrlLength);

        builder.Property(r => r.CoverArtStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(r => new { r.ArtistId, r.MusicBrainzReleaseGroupMbid })
            .IsUnique();

        builder.HasIndex(r => new { r.ArtistId, r.FirstReleaseDate });

        builder.HasOne<Artist>()
            .WithMany()
            .HasForeignKey(r => r.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
