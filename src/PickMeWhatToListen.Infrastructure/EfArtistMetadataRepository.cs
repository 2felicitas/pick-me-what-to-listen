using Microsoft.EntityFrameworkCore;
using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Infrastructure;

public sealed class EfArtistMetadataRepository(IDbContextFactory<AppDbContext> dbContextFactory) : IArtistMetadataRepository
{
    public async Task<ArtistProfileSnapshot?> GetProfileAsync(
        Guid artistId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var artist = await dbContext.Artists.AsNoTracking().FirstOrDefaultAsync(a => a.Id == artistId, cancellationToken);
        if (artist is null)
        {
            return null;
        }

        var terms = await dbContext.ArtistMetadataTerms
            .AsNoTracking()
            .Include(t => t.MetadataTerm)
            .Where(t => t.ArtistId == artistId)
            .OrderByDescending(t => t.VoteCount)
            .ToListAsync(cancellationToken);

        var genres = terms
            .Where(t => t.Kind == MetadataTermKind.Genre)
            .Select(t => new MetadataTermEntry(t.MetadataTerm.DisplayName, t.VoteCount))
            .ToList();

        var tags = terms
            .Where(t => t.Kind == MetadataTermKind.Tag)
            .Select(t => new MetadataTermEntry(t.MetadataTerm.DisplayName, t.VoteCount))
            .ToList();

        var releaseGroups = await dbContext.ReleaseGroups
            .AsNoTracking()
            .Where(r => r.ArtistId == artistId)
            .OrderByDescending(r => r.FirstReleaseDate)
            .ToListAsync(cancellationToken);

        var releaseGroupEntries = releaseGroups
            .Select(r => new ReleaseGroupEntry(
                r.Title,
                r.PrimaryType,
                r.FirstReleaseDate,
                r.CoverArtUrl,
                r.CoverArtStatus))
            .ToList();

        return new ArtistProfileSnapshot(artist, genres, tags, releaseGroupEntries);
    }

    public async Task SaveAsync(ArtistMetadataSaveRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var trackedArtist = await dbContext.Artists.FirstAsync(a => a.Id == request.Artist.Id, cancellationToken);
        dbContext.Entry(trackedArtist).CurrentValues.SetValues(request.Artist);

        var existingTerms = await dbContext.ArtistMetadataTerms
            .Where(t => t.ArtistId == request.Artist.Id)
            .ToListAsync(cancellationToken);
        dbContext.ArtistMetadataTerms.RemoveRange(existingTerms);

        var existingGroups = await dbContext.ReleaseGroups
            .Where(r => r.ArtistId == request.Artist.Id)
            .ToListAsync(cancellationToken);
        dbContext.ReleaseGroups.RemoveRange(existingGroups);

        foreach (var (displayName, kind, voteCount) in request.Terms)
        {
            var key = MetadataTermNormalizer.ToComparisonKey(displayName);
            var term = await dbContext.MetadataTerms.FirstOrDefaultAsync(t => t.Name == key, cancellationToken);
            if (term is null)
            {
                term = MetadataTerm.Create(displayName);
                dbContext.MetadataTerms.Add(term);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            dbContext.ArtistMetadataTerms.Add(
                ArtistMetadataTerm.Create(request.Artist.Id, term.Id, kind, voteCount));
        }

        dbContext.ReleaseGroups.AddRange(request.ReleaseGroups);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingCoverArtItem>> GetPendingCoverArtAsync(
        Guid artistId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.ReleaseGroups
            .AsNoTracking()
            .Where(r => r.ArtistId == artistId && r.CoverArtStatus == CoverArtStatus.None)
            .OrderByDescending(r => r.FirstReleaseDate)
            .Select(r => new PendingCoverArtItem(r.Id, r.MusicBrainzReleaseGroupMbid))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateCoverArtAsync(
        Guid releaseGroupId,
        string? coverArtUrl,
        CoverArtStatus coverArtStatus,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var releaseGroup = await dbContext.ReleaseGroups
            .FirstOrDefaultAsync(r => r.Id == releaseGroupId, cancellationToken);
        if (releaseGroup is null)
        {
            return;
        }

        releaseGroup.SetCoverArt(coverArtUrl, coverArtStatus);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
