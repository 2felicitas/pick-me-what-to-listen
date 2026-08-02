using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Infrastructure;

public sealed class ArtistMetadataTermConfiguration : IEntityTypeConfiguration<ArtistMetadataTerm>
{
    public void Configure(EntityTypeBuilder<ArtistMetadataTerm> builder)
    {
        builder.HasKey(t => new { t.ArtistId, t.MetadataTermId, t.Kind });

        builder.Property(t => t.Kind)
            .HasConversion<string>();

        builder.HasOne(t => t.MetadataTerm)
            .WithMany()
            .HasForeignKey(t => t.MetadataTermId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Artist>()
            .WithMany()
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
