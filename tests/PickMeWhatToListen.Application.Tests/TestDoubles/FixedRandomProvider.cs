using PickMeWhatToListen.Application.Abstractions;

namespace PickMeWhatToListen.Application.Tests.TestDoubles;

/// <summary>Deterministic <see cref="IRandomProvider"/> double that always returns a fixed index.</summary>
public sealed class FixedRandomProvider(int value) : IRandomProvider
{
    public int LastExclusiveUpperBound { get; private set; }

    public int Next(int exclusiveUpperBound)
    {
        LastExclusiveUpperBound = exclusiveUpperBound;
        return value;
    }
}
