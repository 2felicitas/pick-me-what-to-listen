using PickMeWhatToListen.Application;
using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Wpf.ViewModels;

public sealed record MetadataChipViewModel(string DisplayName, int VoteCount);

public sealed record ReleaseGroupRowViewModel(
    string Title,
    string PrimaryTypeLabel,
    string? YearLabel,
    string? CoverArtUrl,
    bool ShowPlaceholder)
{
    public string DisplayLine
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(YearLabel))
            {
                parts.Add(YearLabel);
            }

            parts.Add(PrimaryTypeLabel);
            parts.Add(Title);
            return string.Join(" · ", parts);
        }
    }
}

public sealed record MusicBrainzCandidateViewModel(
    Guid ArtistId,
    string Mbid,
    string Name,
    string? Detail);

public sealed class ArtistProfileViewModel
{
    public ArtistProfileViewModel(
        Artist artist,
        IReadOnlyList<MetadataChipViewModel> genres,
        IReadOnlyList<MetadataChipViewModel> tags,
        IReadOnlyList<ReleaseGroupRowViewModel> releaseGroups,
        IReadOnlyList<MusicBrainzCandidateViewModel> ambiguousCandidates,
        bool isLoading,
        string? statusMessage)
    {
        Id = artist.Id;
        Name = artist.Name;
        IsPicked = artist.IsPicked;
        PickedAtUtc = artist.PickedAtUtc;
        CreatedAtUtc = artist.CreatedAtUtc;
        Genres = genres;
        Tags = tags;
        ReleaseGroups = releaseGroups;
        AmbiguousCandidates = ambiguousCandidates;
        IsLoading = isLoading;
        StatusMessage = statusMessage;
        MetadataSyncedAtUtc = artist.MetadataSyncedAtUtc;
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsPicked { get; }

    public DateTimeOffset? PickedAtUtc { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset? MetadataSyncedAtUtc { get; }

    public IReadOnlyList<MetadataChipViewModel> Genres { get; }

    public IReadOnlyList<MetadataChipViewModel> Tags { get; }

    public IReadOnlyList<ReleaseGroupRowViewModel> ReleaseGroups { get; }

    public IReadOnlyList<MusicBrainzCandidateViewModel> AmbiguousCandidates { get; }

    public bool IsLoading { get; }

    public string? StatusMessage { get; }

    public bool HasGenres => Genres.Count > 0;

    public bool HasTags => Tags.Count > 0;

    public bool HasReleaseGroups => ReleaseGroups.Count > 0;

    public bool ShowAmbiguous => AmbiguousCandidates.Count > 0;

    public static ArtistProfileViewModel Loading(Artist artist) =>
        new(artist, [], [], [], [], isLoading: true, statusMessage: null);

    public static ArtistProfileViewModel FromResult(ArtistProfileResult result)
    {
        if (!result.Found || result.Profile is null)
        {
            throw new InvalidOperationException("Cannot build profile view model from a missing profile.");
        }

        var profile = result.Profile;
        var artist = profile.Artist;

        var genres = profile.Genres
            .Select(g => new MetadataChipViewModel(g.DisplayName, g.VoteCount))
            .ToList();

        var tags = profile.Tags
            .Select(t => new MetadataChipViewModel(t.DisplayName, t.VoteCount))
            .ToList();

        var releaseGroups = profile.ReleaseGroups
            .Select(r => new ReleaseGroupRowViewModel(
                r.Title,
                FormatPrimaryType(r.PrimaryType),
                FormatYear(r.FirstReleaseDate),
                r.CoverArtUrl,
                ShowPlaceholder: r.CoverArtStatus != CoverArtStatus.Ok || string.IsNullOrEmpty(r.CoverArtUrl)))
            .ToList();

        var ambiguous = result.AmbiguousCandidates
            .Select(c => new MusicBrainzCandidateViewModel(
                artist.Id,
                c.Mbid,
                c.Name,
                FormatCandidateDetail(c)))
            .ToList();

        var statusMessage = artist.MetadataSyncStatus switch
        {
            MetadataSyncStatus.NotFound => "Не удалось найти исполнителя в MusicBrainz.",
            MetadataSyncStatus.Failed => artist.MetadataSyncError ?? "Не удалось обновить данные.",
            MetadataSyncStatus.Ambiguous when ambiguous.Count > 0 => "Выберите исполнителя:",
            _ => null,
        };

        return new ArtistProfileViewModel(
            artist,
            genres,
            tags,
            releaseGroups,
            ambiguous,
            isLoading: false,
            statusMessage);
    }

    private static string FormatPrimaryType(string primaryType) =>
        primaryType.Equals("EP", StringComparison.OrdinalIgnoreCase) ? "EP" : "Альбом";

    private static string? FormatYear(string? firstReleaseDate)
    {
        if (string.IsNullOrWhiteSpace(firstReleaseDate))
        {
            return null;
        }

        return firstReleaseDate.Length >= 4 ? firstReleaseDate[..4] : firstReleaseDate;
    }

    private static string? FormatCandidateDetail(MusicBrainzArtistCandidate candidate)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(candidate.Disambiguation))
        {
            parts.Add(candidate.Disambiguation);
        }

        if (!string.IsNullOrWhiteSpace(candidate.Country))
        {
            parts.Add(candidate.Country);
        }

        if (!string.IsNullOrWhiteSpace(candidate.LifeSpanBegin))
        {
            parts.Add(candidate.LifeSpanBegin);
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }
}
