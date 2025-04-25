using EviCache.Enums;

namespace EviCache.Options;

/// <summary>
/// Represents configuration options for the cache.
/// </summary>
public record CacheOptions
{
    /// <summary>
    /// Gets the maximum number of items that the cache can store.
    /// </summary>
    /// <remarks>
    /// When this limit is reached, new items will trigger the configured eviction policy.
    /// </remarks>
    public int Capacity { get; init; }

    /// <summary>
    /// Gets the eviction policy to use.
    /// </summary>
    /// <remarks>
    /// The eviction policy determines which items are removed when the cache is full.
    /// </remarks>
    public EvictionPolicy EvictionPolicy { get; init; }

    /// <summary>
    /// Gets the default Time To Live (TTL) for cache items.
    /// </summary>
    /// <remarks>
    /// If not specified, items will not expire by default.
    /// Individual item TTLs take precedence over this value.
    /// </remarks>
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheOptions"/> record.
    /// </summary>
    /// <param name="capacity">The maximum number of items that the cache can store.</param>
    /// <param name="evictionPolicy">The eviction policy to use when the cache reaches its capacity.</param>
    /// <param name="timeToLive">The default Time To Live (TTL) for cache items.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than or equal to zero.</exception>
    public CacheOptions(int capacity, EvictionPolicy evictionPolicy)
    {
        Capacity = capacity;
        EvictionPolicy = evictionPolicy;
    }

    /// <inheritdoc cref="CacheOptions(int,EvictionPolicy)" />
    public CacheOptions(int capacity, EvictionPolicy evictionPolicy, TimeSpan timeToLive)
    {
        Capacity = capacity;
        EvictionPolicy = evictionPolicy;
        TimeToLive = timeToLive;
    }
}