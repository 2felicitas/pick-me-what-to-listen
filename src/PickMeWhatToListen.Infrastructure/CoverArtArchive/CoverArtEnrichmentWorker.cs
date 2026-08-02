using System.Threading.Channels;
using PickMeWhatToListen.Application;
using PickMeWhatToListen.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PickMeWhatToListen.Infrastructure.CoverArtArchive;

public sealed class CoverArtEnrichmentCoordinator : ICoverArtEnrichmentQueue, ICoverArtEnrichmentNotifier
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    private readonly HashSet<Guid> _queuedOrRunning = [];
    private readonly Lock _gate = new();
    private readonly List<Action<Guid>> _handlers = [];

    internal ChannelReader<Guid> Reader => _channel.Reader;

    public void EnqueueArtist(Guid artistId)
    {
        lock (_gate)
        {
            if (!_queuedOrRunning.Add(artistId))
            {
                return;
            }
        }

        _channel.Writer.TryWrite(artistId);
    }

    internal void MarkArtistCompleted(Guid artistId)
    {
        lock (_gate)
        {
            _queuedOrRunning.Remove(artistId);
        }
    }

    public void Register(Action<Guid> handler)
    {
        lock (_gate)
        {
            _handlers.Add(handler);
        }
    }

    public void NotifyArtistCoverArtUpdated(Guid artistId)
    {
        Action<Guid>[] snapshot;
        lock (_gate)
        {
            snapshot = [.. _handlers];
        }

        foreach (var handler in snapshot)
        {
            handler(artistId);
        }
    }
}

public sealed class CoverArtEnrichmentWorker(
    CoverArtEnrichmentCoordinator coordinator,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var artistId in coordinator.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var enrichment = scope.ServiceProvider.GetRequiredService<CoverArtEnrichmentService>();
                await enrichment.EnrichArtistCoversAsync(artistId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                // Best-effort background work — profile text is already cached.
            }
            finally
            {
                coordinator.MarkArtistCompleted(artistId);
            }
        }
    }
}
