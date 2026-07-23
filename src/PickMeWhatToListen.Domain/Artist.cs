namespace PickMeWhatToListen.Domain;

/// <summary>
/// An artist in the listening catalog. <see cref="IsPicked"/> is a persistent
/// "already listened to / already picked" marker, not a single current selection:
/// once set it stays set, and random draws only consider unpicked artists.
/// </summary>
public sealed class Artist
{
    public const int MaxNameLength = 200;

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public bool IsPicked { get; private set; }

    public DateTimeOffset? PickedAtUtc { get; private set; }

    private Artist(Guid id, string name, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Name = name;
        CreatedAtUtc = createdAtUtc;
    }

    // Required by EF Core for materialization.
    private Artist()
    {
        Name = string.Empty;
    }

    public static Artist Create(string name, DateTimeOffset? createdAtUtc = null)
    {
        var normalizedName = NormalizeName(name);
        return new Artist(Guid.NewGuid(), normalizedName, createdAtUtc ?? DateTimeOffset.UtcNow);
    }

    public void Pick(DateTimeOffset? pickedAtUtc = null)
    {
        if (IsPicked)
        {
            throw new InvalidOperationException($"Artist '{Name}' has already been picked.");
        }

        IsPicked = true;
        PickedAtUtc = pickedAtUtc ?? DateTimeOffset.UtcNow;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Artist name cannot be empty.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException($"Artist name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        return trimmed;
    }
}
