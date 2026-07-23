using PickMeWhatToListen.Domain;

namespace PickMeWhatToListen.Application.Abstractions;

/// <summary>
/// Persistence port for <see cref="Artist"/>. Implemented in the Infrastructure
/// layer; nothing outside Infrastructure may reference EF Core types directly.
/// </summary>
public interface IArtistRepository
{
    public Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Artist>> GetUnpickedAsync(CancellationToken cancellationToken = default);

    public Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task AddAsync(Artist artist, CancellationToken cancellationToken = default);

    public Task UpdateAsync(Artist artist, CancellationToken cancellationToken = default);
}
