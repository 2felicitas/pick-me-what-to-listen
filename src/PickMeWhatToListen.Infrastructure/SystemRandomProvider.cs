using PickMeWhatToListen.Application.Abstractions;

namespace PickMeWhatToListen.Infrastructure;

public sealed class SystemRandomProvider : IRandomProvider
{
    public int Next(int exclusiveUpperBound) => Random.Shared.Next(exclusiveUpperBound);
}
