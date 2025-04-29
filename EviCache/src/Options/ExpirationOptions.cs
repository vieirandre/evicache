using EviCache.Enums;

namespace EviCache.Options;

/// <summary>
/// Represents configuration options for cache item expiration.
/// </summary>
public record ExpirationOptions
{
    /// <summary>
    /// Gets the duration after which the cache item expires, if specified; otherwise, null.
    /// </summary>
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>
    /// Gets the expiration mode, determining whether it is absolute or sliding.
    /// </summary>
    public ExpirationMode Mode { get; init; } = ExpirationMode.Absolute;
}
