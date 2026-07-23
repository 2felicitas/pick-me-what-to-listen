using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application.Tests.TestDoubles;

/// <summary>In-memory <see cref="IArtistRepository"/> double for fast, DB-free unit tests.</summary>
public sealed class FakeArtistRepository : IArtistRepository
{
    private readonly List<Artist> _artists = [];

    public FakeArtistRepository Seed(params Artist[] artists)
    {
        _artists.AddRange(artists);
        return this;
    }

    public Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Artist>>([.. _artists]);

    public Task<IReadOnlyList<Artist>> GetUnpickedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Artist>>([.. _artists.Where(a => !a.IsPicked)]);

    public Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_artists.SingleOrDefault(a => a.Id == id));

    public Task AddAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        _artists.Add(artist);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        // The fake stores references, so mutations via Artist methods are already visible;
        // this mirrors the "persist" call an EF-backed repository would require.
        return Task.CompletedTask;
    }
}
