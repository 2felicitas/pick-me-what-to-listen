using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application;

public static class ArtistMetadataRules
{
    public const int MinVoteCount = 5;

    /// <summary>Max album/EP rows stored and shown in the profile panel.</summary>
    public const int MaxReleaseGroups = 50;

    /// <summary>
    /// MusicBrainz sets <c>secondary-types</c> for compilations, live albums, soundtracks, etc.
    /// Skip those — only plain album/EP rows (empty secondary-types) belong in discography.
    /// </summary>
    public static bool IsDiscographyReleaseGroup(IReadOnlyList<string> secondaryTypes) =>
        secondaryTypes.Count == 0;

    public static IReadOnlyList<MusicBrainzReleaseGroupData> SelectDiscographyForSync(
        IEnumerable<MusicBrainzReleaseGroupData> releaseGroups) =>
        releaseGroups
            .Where(g => IsDiscographyReleaseGroup(g.SecondaryTypes))
            .OrderByDescending(g => ParsePartialDate(g.FirstReleaseDate))
            .ThenByDescending(g => g.FirstReleaseDate ?? string.Empty, StringComparer.Ordinal)
            .Take(MaxReleaseGroups)
            .ToList();

    public static IReadOnlyList<(string DisplayName, MetadataTermKind Kind, int VoteCount)> BuildTerms(
        IReadOnlyList<MusicBrainzVoteTerm> genres,
        IReadOnlyList<MusicBrainzVoteTerm> tags)
    {
        var filteredGenres = genres
            .Where(g => g.Count >= MinVoteCount)
            .OrderByDescending(g => g.Count)
            .ToList();

        var genreKeys = new HashSet<string>(
            filteredGenres.Select(g => MetadataTermNormalizer.ToComparisonKey(g.Name)),
            StringComparer.Ordinal);

        var filteredTags = tags
            .Where(t => t.Count >= MinVoteCount)
            .Where(t => !genreKeys.Contains(MetadataTermNormalizer.ToComparisonKey(t.Name)))
            .OrderByDescending(t => t.Count)
            .ToList();

        var terms = new List<(string, MetadataTermKind, int)>();
        terms.AddRange(filteredGenres.Select(g => (g.Name, MetadataTermKind.Genre, g.Count)));
        terms.AddRange(filteredTags.Select(t => (t.Name, MetadataTermKind.Tag, t.Count)));
        return terms;
    }

    public static MusicBrainzReleaseData? PickEarliestRelease(IReadOnlyList<MusicBrainzReleaseData> releases)
    {
        if (releases.Count == 0)
        {
            return null;
        }

        return releases
            .OrderBy(r => ParsePartialDate(r.Date))
            .ThenBy(r => r.Date ?? string.Empty, StringComparer.Ordinal)
            .First();
    }

    public static (int Year, int Month, int Day) ParsePartialDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return (9999, 12, 31);
        }

        var parts = date.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var year = parts.Length > 0 && int.TryParse(parts[0], out var y) ? y : 9999;
        var month = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : 1;
        var day = parts.Length > 2 && int.TryParse(parts[2], out var d) ? d : 1;
        return (year, month, day);
    }
}
