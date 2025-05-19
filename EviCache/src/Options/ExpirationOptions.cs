namespace EviCache.Options;

/// <summary>
/// Represents configuration options for cache item expiration.
/// </summary>
public abstract record ExpirationOptions
{
    private ExpirationOptions() { }

    public sealed record Absolute(TimeSpan TimeToLive) : ExpirationOptions;
    public sealed record Sliding(TimeSpan TimeToLive) : ExpirationOptions;
    public sealed record None : ExpirationOptions;
}
