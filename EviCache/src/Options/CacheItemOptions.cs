namespace EviCache.Options;

/// <summary>
/// Represents configuration options for a cache item.
/// </summary>
public record CacheItemOptions
{
    /// <summary>
    /// Gets the expiration options for the cache item, if specified; otherwise, null.
    /// </summary>
    public ExpirationOptions? Expiration { get; init; }
}
