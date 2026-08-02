namespace PickMeWhatToListen.Domain;

public sealed class ReleaseGroup
{
    public const int MaxTitleLength = 500;
    public const int MaxMbidLength = 36;
    public const int MaxDateLength = 10;
    public const int MaxCoverUrlLength = 2048;

    public Guid Id { get; private set; }

    public Guid ArtistId { get; private set; }

    public string MusicBrainzReleaseGroupMbid { get; private set; }

    public string Title { get; private set; }

    public string PrimaryType { get; private set; }

    public string? FirstReleaseDate { get; private set; }

    public string? CoverReleaseMbid { get; private set; }

    public string? CoverArtUrl { get; private set; }

    public CoverArtStatus CoverArtStatus { get; private set; }

    private ReleaseGroup(
        Guid id,
        Guid artistId,
        string musicBrainzReleaseGroupMbid,
        string title,
        string primaryType,
        string? firstReleaseDate,
        string? coverReleaseMbid,
        string? coverArtUrl,
        CoverArtStatus coverArtStatus)
    {
        Id = id;
        ArtistId = artistId;
        MusicBrainzReleaseGroupMbid = musicBrainzReleaseGroupMbid;
        Title = title;
        PrimaryType = primaryType;
        FirstReleaseDate = firstReleaseDate;
        CoverReleaseMbid = coverReleaseMbid;
        CoverArtUrl = coverArtUrl;
        CoverArtStatus = coverArtStatus;
    }

    // Required by EF Core for materialization.
    private ReleaseGroup()
    {
        MusicBrainzReleaseGroupMbid = string.Empty;
        Title = string.Empty;
        PrimaryType = string.Empty;
    }

    public void SetCoverArt(string? coverArtUrl, CoverArtStatus coverArtStatus)
    {
        if (coverArtUrl is not null && coverArtUrl.Length > MaxCoverUrlLength)
        {
            throw new ArgumentException($"Cover art URL cannot exceed {MaxCoverUrlLength} characters.", nameof(coverArtUrl));
        }

        CoverArtUrl = coverArtUrl;
        CoverArtStatus = coverArtStatus;
    }

    public static ReleaseGroup Create(
        Guid artistId,
        string musicBrainzReleaseGroupMbid,
        string title,
        string primaryType,
        string? firstReleaseDate,
        string? coverReleaseMbid,
        string? coverArtUrl,
        CoverArtStatus coverArtStatus)
    {
        var trimmedTitle = title.Trim();
        if (string.IsNullOrEmpty(trimmedTitle))
        {
            throw new ArgumentException("Release group title cannot be empty.", nameof(title));
        }

        if (trimmedTitle.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Release group title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        ValidateMbid(musicBrainzReleaseGroupMbid, nameof(musicBrainzReleaseGroupMbid));
        if (coverReleaseMbid is not null)
        {
            ValidateMbid(coverReleaseMbid, nameof(coverReleaseMbid));
        }

        if (coverArtUrl is not null && coverArtUrl.Length > MaxCoverUrlLength)
        {
            throw new ArgumentException($"Cover art URL cannot exceed {MaxCoverUrlLength} characters.", nameof(coverArtUrl));
        }

        if (firstReleaseDate is not null && firstReleaseDate.Length > MaxDateLength)
        {
            throw new ArgumentException($"First release date cannot exceed {MaxDateLength} characters.", nameof(firstReleaseDate));
        }

        return new ReleaseGroup(
            Guid.NewGuid(),
            artistId,
            musicBrainzReleaseGroupMbid,
            trimmedTitle,
            primaryType,
            firstReleaseDate,
            coverReleaseMbid,
            coverArtUrl,
            coverArtStatus);
    }

    private static void ValidateMbid(string mbid, string paramName)
    {
        if (string.IsNullOrWhiteSpace(mbid) || mbid.Length > MaxMbidLength)
        {
            throw new ArgumentException("Invalid MusicBrainz ID.", paramName);
        }
    }
}
