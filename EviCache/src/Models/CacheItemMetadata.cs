using EviCache.Options;

namespace EviCache.Models;

/// <summary>
/// Represents metadata for a cache item.
/// </summary>
public sealed class CacheItemMetadata
{
    /// <summary>
    /// Gets the time when the cache item was last accessed.
    /// </summary>
    public DateTimeOffset LastAccessedAt { get; private set; }

    /// <summary>
    /// Gets the time when the cache item was last updated.
    /// </summary>
    public DateTimeOffset? LastUpdatedAt { get; private set; }

    /// <summary>
    /// Gets the number of times this item has been accessed.
    /// </summary>
    public long AccessCount => Interlocked.Read(ref _accessCount);
    private long _accessCount;

    /// <summary>
    /// Gets the date and time when the cache item expires, if absolute expiration is set; otherwise, null.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the cache item has expired based on its expiration settings.
    /// </summary>
    public bool IsExpired
    {
        get
        {
            var now = DateTimeOffset.UtcNow;

            return Expiration switch
            {
                ExpirationOptions.Absolute _
                    => ExpiresAt is { } ts && now > ts,
                ExpirationOptions.Sliding s
                    => now - LastAccessedAt > s.TimeToLive,
                _ => false
            };
        }
    }

    /// <summary>
    /// Gets the expiration settings for the cache item, if configured; otherwise, null.
    /// </summary>
    public ExpirationOptions? Expiration { get; internal set; }

    internal CacheItemMetadata()
    {
        LastAccessedAt = DateTimeOffset.UtcNow;
        _accessCount = 0;
    }

    internal CacheItemMetadata(ExpirationOptions expirationOptions) : this()
    {
        Expiration = expirationOptions;
    }

    internal void RegisterAccess()
    {
        LastAccessedAt = DateTimeOffset.UtcNow;
        Interlocked.Increment(ref _accessCount);
    }

    internal void RegisterUpdate()
    {
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }
}
