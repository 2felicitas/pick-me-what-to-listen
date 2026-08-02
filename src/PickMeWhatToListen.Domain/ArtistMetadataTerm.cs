namespace PickMeWhatToListen.Domain;

public sealed class ArtistMetadataTerm
{
    public Guid ArtistId { get; private set; }

    public int MetadataTermId { get; private set; }

    public MetadataTermKind Kind { get; private set; }

    public int VoteCount { get; private set; }

    public MetadataTerm MetadataTerm { get; private set; } = null!;

    private ArtistMetadataTerm(Guid artistId, int metadataTermId, MetadataTermKind kind, int voteCount)
    {
        ArtistId = artistId;
        MetadataTermId = metadataTermId;
        Kind = kind;
        VoteCount = voteCount;
    }

    // Required by EF Core for materialization.
    private ArtistMetadataTerm()
    {
    }

    public static ArtistMetadataTerm Create(
        Guid artistId,
        int metadataTermId,
        MetadataTermKind kind,
        int voteCount)
    {
        if (voteCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(voteCount));
        }

        return new ArtistMetadataTerm(artistId, metadataTermId, kind, voteCount);
    }
}
