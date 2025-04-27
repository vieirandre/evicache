using EviCache.Enums;
using EviCache.Options;

namespace EviCache.Models;

/// <summary>
/// Represents metadata for a cache item.
/// </summary>
public class CacheItemMetadata
{
    /// <summary>
    /// Gets the time when the cache item was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the time when the cache item was last accessed.
    /// </summary>
    public DateTimeOffset LastAccessedAt { get; private set; }

    /// <summary>
    /// Gets the time when the cache item was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; private set; }

    /// <summary>
    /// Gets the number of times this item has been accessed.
    /// </summary>
    public long AccessCount => Interlocked.Read(ref _accessCount);
    private long _accessCount;

    public DateTimeOffset? ExpiresAt { get; internal set; }

    public bool IsExpired
    {
        get
        {
            return Expiration?.Mode switch
            {
                ExpirationMode.Absolute when ExpiresAt.HasValue => ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt.Value,
                ExpirationMode.Sliding when Expiration.TimeToLive.HasValue => DateTimeOffset.UtcNow > LastAccessedAt + Expiration.TimeToLive.Value,
                _ => false
            };
        }
    }

    public ExpirationOptions? Expiration { get; internal set; }

    internal CacheItemMetadata()
    {
        var now = DateTimeOffset.UtcNow;
        CreatedAt = now;
        LastAccessedAt = now;
        LastUpdatedAt = now;
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
