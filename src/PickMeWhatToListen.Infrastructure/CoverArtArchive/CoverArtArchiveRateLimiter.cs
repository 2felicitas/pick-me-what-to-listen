namespace PickMeWhatToListen.Infrastructure.CoverArtArchive;

/// <summary>Light throttling for Cover Art Archive requests during bulk discography sync.</summary>
public sealed class CoverArtArchiveRateLimiter
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTimeOffset _lastRequestUtc = DateTimeOffset.MinValue;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var elapsed = DateTimeOffset.UtcNow - _lastRequestUtc;
            if (elapsed < TimeSpan.FromMilliseconds(200))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200) - elapsed, cancellationToken);
            }

            _lastRequestUtc = DateTimeOffset.UtcNow;
        }
        finally
        {
            _gate.Release();
        }
    }
}
