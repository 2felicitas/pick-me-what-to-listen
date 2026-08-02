namespace PickMeWhatToListen.Domain;

/// <summary>Canonical name for a MusicBrainz genre or tag label.</summary>
public sealed class MetadataTerm
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public string DisplayName { get; private set; }

    private MetadataTerm(int id, string name, string displayName)
    {
        Id = id;
        Name = name;
        DisplayName = displayName;
    }

    // Required by EF Core for materialization.
    private MetadataTerm()
    {
        Name = string.Empty;
        DisplayName = string.Empty;
    }

    public static MetadataTerm Create(string displayName)
    {
        var trimmed = displayName.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ArgumentException("Metadata term display name cannot be empty.", nameof(displayName));
        }

        return new MetadataTerm(0, MetadataTermNormalizer.ToComparisonKey(trimmed), trimmed);
    }
}
