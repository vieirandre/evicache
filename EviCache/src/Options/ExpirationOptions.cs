using EviCache.Enums;

namespace EviCache.Options;

public record ExpirationOptions
{
    public TimeSpan? TimeToLive { get; init; }
    public ExpirationMode Mode { get; init; } = ExpirationMode.Absolute;
}
