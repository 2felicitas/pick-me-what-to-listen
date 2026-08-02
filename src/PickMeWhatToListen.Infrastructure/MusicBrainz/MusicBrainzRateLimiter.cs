namespace PickMeWhatToListen.Infrastructure.MusicBrainz;

/// <summary>Enforces MusicBrainz's 1 request per second policy across all MB API calls.</summary>
public sealed class MusicBrainzRateLimiter
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTimeOffset _lastRequestUtc = DateTimeOffset.MinValue;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var elapsed = DateTimeOffset.UtcNow - _lastRequestUtc;
            if (elapsed < TimeSpan.FromMilliseconds(1100))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1100) - elapsed, cancellationToken);
            }

            _lastRequestUtc = DateTimeOffset.UtcNow;
        }
        finally
        {
            _gate.Release();
        }
    }
}
